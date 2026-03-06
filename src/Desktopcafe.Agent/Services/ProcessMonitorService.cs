using System.Diagnostics;
using Serilog;

namespace Desktopcafe.Agent.Services;

public class ProcessMonitorService
{
    private readonly HashSet<string> _blockedProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "taskmgr",  // Block task manager to prevent killing agent
        "regedit",  // Block registry editor
        "msconfig"  // Block system config
    };

    private CancellationTokenSource _cts = new();

    public async Task StartMonitoringAsync()
    {
        _cts = new CancellationTokenSource();
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(_cts.Token);
                KillBlockedProcesses();
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { Log.Error(ex, "Process monitor error"); }
        }
    }

    private void KillBlockedProcesses()
    {
        foreach (var name in _blockedProcesses)
        {
            try
            {
                var processes = Process.GetProcessesByName(name);
                foreach (var proc in processes)
                {
                    proc.Kill();
                    Log.Information("Killed blocked process: {Name}", name);
                }
            }
            catch (Exception ex) { Log.Warning(ex, "Failed to kill blocked process {Name}", name); }
        }
    }

    public void Stop() => _cts.Cancel();

    public void AddBlockedProcess(string name) => _blockedProcesses.Add(name);
    public void RemoveBlockedProcess(string name) => _blockedProcesses.Remove(name);
}
