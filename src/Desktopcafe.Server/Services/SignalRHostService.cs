using System.Collections.Concurrent;
using Desktopcafe.Core.DTOs;
using Desktopcafe.Shared;
using Desktopcafe.Shared.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Desktopcafe.Server.Services;

public class SignalRHostService : IHostedService, IDisposable
{
    private WebApplication? _app;
    private readonly ILogger<SignalRHostService> _logger;

    public SignalRHostService(ILogger<SignalRHostService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(Constants.ServerPort);
        });

        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(Constants.ClientTimeoutSeconds);
        }).AddMessagePackProtocol();

        builder.Services.AddSingleton<CafeHub>();

        builder.Logging.ClearProviders();

        _app = builder.Build();

        _app.MapHub<CafeHub>(Constants.HubPath);

        _logger.LogInformation("Starting SignalR host on port {Port}", Constants.ServerPort);
        await _app.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_app is not null)
        {
            _logger.LogInformation("Stopping SignalR host");
            await _app.StopAsync(cancellationToken);
        }
    }

    public void Dispose()
    {
        _app?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }
}

public class CafeHub : Hub<IAgentHub>, IServerHub
{
    private static readonly ConcurrentDictionary<string, string> _connectionToMac = new();
    private static readonly ConcurrentDictionary<string, string> _macToConnection = new();

    private readonly ILogger<CafeHub> _logger;

    public CafeHub(ILogger<CafeHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Agent connected: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectionToMac.TryRemove(Context.ConnectionId, out var mac))
        {
            _macToConnection.TryRemove(mac, out _);
            _logger.LogInformation("Agent disconnected: {Mac} ({ConnectionId})", mac, Context.ConnectionId);
        }
        return base.OnDisconnectedAsync(exception);
    }

    // --- IServerHub implementation ---

    public Task Register(ComputerRegistrationDto info)
    {
        var connectionId = Context.ConnectionId;
        _connectionToMac[connectionId] = info.MacAddress;
        _macToConnection[info.MacAddress] = connectionId;
        _logger.LogInformation("Agent registered: {Name} ({Mac})", info.Name, info.MacAddress);
        return Task.CompletedTask;
    }

    public Task Heartbeat(HeartbeatDto status)
    {
        // Update connection mapping if needed
        if (!_connectionToMac.ContainsKey(Context.ConnectionId) && !string.IsNullOrEmpty(status.MacAddress))
        {
            _connectionToMac[Context.ConnectionId] = status.MacAddress;
            _macToConnection[status.MacAddress] = Context.ConnectionId;
        }
        return Task.CompletedTask;
    }

    public Task PrintJobDetected(PrintJobDto job)
    {
        _logger.LogInformation("Print job detected: {Doc} ({Pages} pages)", job.DocumentName, job.Pages);
        return Task.CompletedTask;
    }

    public Task SessionRequest(SessionRequestDto request)
    {
        _logger.LogInformation("Session request from {Mac}", request.MacAddress);
        return Task.CompletedTask;
    }

    public Task AlertAdmin(AlertDto alert)
    {
        _logger.LogWarning("Agent alert [{Severity}]: {Title} - {Message}", alert.Severity, alert.Title, alert.Message);
        return Task.CompletedTask;
    }

    public Task SendScreenshot(byte[] imageData)
    {
        _logger.LogInformation("Screenshot received ({Size} bytes) from {ConnectionId}",
            imageData.Length, Context.ConnectionId);
        return Task.CompletedTask;
    }

    public Task GameLaunched(int gameId)
    {
        _logger.LogInformation("Game {GameId} launched on {ConnectionId}", gameId, Context.ConnectionId);
        return Task.CompletedTask;
    }

    // --- Send commands to specific agents by MAC address ---

    public async Task SendLockScreen(string macAddress)
    {
        if (_macToConnection.TryGetValue(macAddress, out var connectionId))
            await Clients.Client(connectionId).LockScreen();
    }

    public async Task SendUnlockScreen(string macAddress, SessionDto session)
    {
        if (_macToConnection.TryGetValue(macAddress, out var connectionId))
            await Clients.Client(connectionId).UnlockScreen(session);
    }

    public async Task SendUpdateTimer(string macAddress, TimeSpan remaining, double progress)
    {
        if (_macToConnection.TryGetValue(macAddress, out var connectionId))
            await Clients.Client(connectionId).UpdateTimer(remaining, progress);
    }

    public async Task SendShowWarning(string macAddress, string message, int minutesLeft)
    {
        if (_macToConnection.TryGetValue(macAddress, out var connectionId))
            await Clients.Client(connectionId).ShowWarning(message, minutesLeft);
    }

    public async Task SendShowMessage(string macAddress, string title, string message)
    {
        if (_macToConnection.TryGetValue(macAddress, out var connectionId))
            await Clients.Client(connectionId).ShowMessage(title, message);
    }

    public async Task SendShutdown(string macAddress)
    {
        if (_macToConnection.TryGetValue(macAddress, out var connectionId))
            await Clients.Client(connectionId).Shutdown();
    }

    public async Task SendRestart(string macAddress)
    {
        if (_macToConnection.TryGetValue(macAddress, out var connectionId))
            await Clients.Client(connectionId).Restart();
    }

    public async Task SendRequestScreenshot(string macAddress)
    {
        if (_macToConnection.TryGetValue(macAddress, out var connectionId))
            await Clients.Client(connectionId).RequestScreenshot();
    }

    public async Task SendUpdateGameCatalog(string macAddress, List<GameDto> games)
    {
        if (_macToConnection.TryGetValue(macAddress, out var connectionId))
            await Clients.Client(connectionId).UpdateGameCatalog(games);
    }

    public async Task SendUpdateConfig(string macAddress, AgentConfigDto config)
    {
        if (_macToConnection.TryGetValue(macAddress, out var connectionId))
            await Clients.Client(connectionId).UpdateConfig(config);
    }

    public async Task SendBatchUpdateTimers(List<TimerUpdateDto> updates)
    {
        foreach (var update in updates)
        {
            if (_macToConnection.TryGetValue(update.MacAddress, out var connectionId))
                await Clients.Client(connectionId).UpdateTimer(update.TimeRemaining, update.Progress);
        }
    }

    // --- Utility ---

    public static bool IsAgentConnected(string macAddress) =>
        _macToConnection.ContainsKey(macAddress);

    public static IReadOnlyDictionary<string, string> ConnectedAgents => _macToConnection;
}
