using System.Net.NetworkInformation;
using Desktopcafe.Core.DTOs;
using Desktopcafe.Shared;
using Desktopcafe.Shared.Hubs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Desktopcafe.Agent.Services;

public class SignalRClientService : IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly ScreenLockService _lockService;
    private readonly HardwareInfoService _hwInfo;
    private readonly PeriodicTimer _heartbeatTimer = new(TimeSpan.FromSeconds(Constants.HeartbeatIntervalSeconds));
    private CancellationTokenSource _cts = new();
    private string _serverIp = "192.168.1.1";

    public event Action<SessionDto>? SessionStarted;
    public event Action? SessionEnded;
    public event Action<TimeSpan, double>? TimerUpdated;
    public event Action<string, int>? WarningReceived;
    public event Action<List<GameDto>>? GameCatalogUpdated;
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public SignalRClientService(ScreenLockService lockService, HardwareInfoService hwInfo)
    {
        _lockService = lockService;
        _hwInfo = hwInfo;
    }

    public async Task ConnectAsync(string? serverIp = null)
    {
        if (serverIp != null) _serverIp = serverIp;

        _connection = new HubConnectionBuilder()
            .WithUrl($"http://{_serverIp}:{Constants.ServerPort}{Constants.HubPath}")
            .AddMessagePackProtocol()
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60) })
            .Build();

        RegisterHandlers();

        _connection.Reconnecting += _ => { Log.Information("Reconnecting to server..."); return Task.CompletedTask; };
        _connection.Reconnected += _ => RegisterWithServerAsync();
        _connection.Closed += _ => { Log.Warning("Connection closed"); return Task.CompletedTask; };

        await _connection.StartAsync();
        await RegisterWithServerAsync();
        _ = HeartbeatLoopAsync(_cts.Token);
    }

    private void RegisterHandlers()
    {
        _connection!.On("LockScreen", () =>
        {
            _lockService.Lock();
            SessionEnded?.Invoke();
        });

        _connection.On<SessionDto>("UnlockScreen", session =>
        {
            _lockService.Unlock();
            SessionStarted?.Invoke(session);
        });

        _connection.On<TimeSpan, double>("UpdateTimer", (remaining, progress) =>
        {
            TimerUpdated?.Invoke(remaining, progress);
        });

        _connection.On<string, int>("ShowWarning", (message, mins) =>
        {
            WarningReceived?.Invoke(message, mins);
        });

        _connection.On<string, string>("ShowMessage", (title, msg) =>
        {
            Log.Information("Message from server: {Title} - {Msg}", title, msg);
        });

        _connection.On("Shutdown", () =>
        {
            System.Diagnostics.Process.Start("shutdown", "/s /t 10");
        });

        _connection.On("Restart", () =>
        {
            System.Diagnostics.Process.Start("shutdown", "/r /t 10");
        });

        _connection.On<List<GameDto>>("UpdateGameCatalog", games =>
        {
            GameCatalogUpdated?.Invoke(games);
        });

        _connection.On<AgentConfigDto>("UpdateConfig", config =>
        {
            Log.Information("Config updated from server");
        });
    }

    private async Task RegisterWithServerAsync()
    {
        if (_connection?.State != HubConnectionState.Connected) return;

        var mac = GetMacAddress();
        var registration = new ComputerRegistrationDto(
            Name: Environment.MachineName,
            IpAddress: _hwInfo.GetLocalIpAddress(),
            MacAddress: mac,
            Specs: _hwInfo.GetSystemSpecs());

        await _connection.InvokeAsync("Register", registration);
        Log.Information("Registered with server as {Name}", Environment.MachineName);
    }

    private async Task HeartbeatLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await _heartbeatTimer.WaitForNextTickAsync(ct);
                if (_connection?.State == HubConnectionState.Connected)
                {
                    var heartbeat = new HeartbeatDto(
                        MacAddress: GetMacAddress(),
                        CpuUsage: _hwInfo.GetCpuUsage(),
                        RamUsage: _hwInfo.GetRamUsage(),
                        DiskUsage: _hwInfo.GetDiskUsage());

                    await _connection.InvokeAsync("Heartbeat", heartbeat, ct);
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { Log.Error(ex, "Heartbeat failed"); }
        }
    }

    public async Task RequestExtensionAsync()
    {
        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync("SessionRequest", new SessionRequestDto(GetMacAddress(), null));
        }
    }

    public async Task AlertAdminAsync(string title, string message)
    {
        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync("AlertAdmin", new AlertDto(GetMacAddress(), title, message, "Warning"));
        }
    }

    private static string GetMacAddress()
    {
        var nic = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);
        return nic?.GetPhysicalAddress().ToString() ?? "000000000000";
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        if (_connection != null)
            await _connection.DisposeAsync();
    }
}
