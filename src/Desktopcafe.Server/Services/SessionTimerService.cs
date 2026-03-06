using System.Collections.Concurrent;
using Desktopcafe.Shared;
using Serilog;

namespace Desktopcafe.Server.Services;

public record ActiveSession
{
    public int SessionId { get; init; }
    public int ComputerId { get; init; }
    public string MacAddress { get; init; } = string.Empty;
    public TimeSpan TimeRemaining { get; set; }
    public TimeSpan TotalDuration { get; init; }
    public bool IsPaused { get; set; }
    public bool Warned5Min { get; set; }
    public bool Warned2Min { get; set; }
    public bool Warned1Min { get; set; }
}

public class SessionWarningEventArgs : EventArgs
{
    public int SessionId { get; init; }
    public string MacAddress { get; init; } = string.Empty;
    public int MinutesLeft { get; init; }
}

public class SessionExpiredEventArgs : EventArgs
{
    public int SessionId { get; init; }
    public string MacAddress { get; init; } = string.Empty;
}

public class SessionTimerService : IDisposable
{
    private readonly ConcurrentDictionary<int, ActiveSession> _sessions = new();
    private readonly PeriodicTimer _timer;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _timerTask;

    public event EventHandler<SessionWarningEventArgs>? SessionWarning;
    public event EventHandler<SessionExpiredEventArgs>? SessionExpired;

    public IReadOnlyDictionary<int, ActiveSession> ActiveSessions => _sessions;

    public SessionTimerService()
    {
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        _timerTask = RunTimerLoopAsync();
    }

    private async Task RunTimerLoopAsync()
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(_cts.Token))
            {
                Tick();
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown requested
        }
    }

    private void Tick()
    {
        foreach (var kvp in _sessions)
        {
            try
            {
                var session = kvp.Value;
                if (session.IsPaused) continue;

                session.TimeRemaining -= TimeSpan.FromSeconds(1);

                if (session.TimeRemaining <= TimeSpan.Zero)
                {
                    session.TimeRemaining = TimeSpan.Zero;
                    _sessions.TryRemove(kvp.Key, out _);
                    SessionExpired?.Invoke(this, new SessionExpiredEventArgs
                    {
                        SessionId = session.SessionId,
                        MacAddress = session.MacAddress
                    });
                    continue;
                }

                CheckWarnings(session);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to process timer tick for session {SessionId}", kvp.Key);
            }
        }
    }

    private void CheckWarnings(ActiveSession session)
    {
        var minutesLeft = session.TimeRemaining.TotalMinutes;

        if (!session.Warned5Min && minutesLeft <= Constants.Defaults.WarningMinutes1)
        {
            session.Warned5Min = true;
            RaiseWarning(session, Constants.Defaults.WarningMinutes1);
        }
        else if (!session.Warned2Min && minutesLeft <= Constants.Defaults.WarningMinutes2)
        {
            session.Warned2Min = true;
            RaiseWarning(session, Constants.Defaults.WarningMinutes2);
        }
        else if (!session.Warned1Min && minutesLeft <= Constants.Defaults.WarningMinutes3)
        {
            session.Warned1Min = true;
            RaiseWarning(session, Constants.Defaults.WarningMinutes3);
        }
    }

    private void RaiseWarning(ActiveSession session, int minutesLeft)
    {
        SessionWarning?.Invoke(this, new SessionWarningEventArgs
        {
            SessionId = session.SessionId,
            MacAddress = session.MacAddress,
            MinutesLeft = minutesLeft
        });
    }

    public void AddSession(int sessionId, int computerId, string macAddress, TimeSpan duration)
    {
        var session = new ActiveSession
        {
            SessionId = sessionId,
            ComputerId = computerId,
            MacAddress = macAddress,
            TimeRemaining = duration,
            TotalDuration = duration
        };
        _sessions[sessionId] = session;
    }

    public void RemoveSession(int sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
    }

    public void PauseSession(int sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.IsPaused = true;
        }
    }

    public void ResumeSession(int sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.IsPaused = false;
        }
    }

    public void ExtendSession(int sessionId, TimeSpan additionalTime)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.TimeRemaining += additionalTime;
            // Reset warnings so they can fire again if applicable
            session.Warned5Min = session.TimeRemaining.TotalMinutes > Constants.Defaults.WarningMinutes1;
            session.Warned2Min = session.TimeRemaining.TotalMinutes > Constants.Defaults.WarningMinutes2;
            session.Warned1Min = session.TimeRemaining.TotalMinutes > Constants.Defaults.WarningMinutes3;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _timer.Dispose();
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}
