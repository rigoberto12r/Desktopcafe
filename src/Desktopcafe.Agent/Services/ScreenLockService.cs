using Serilog;

namespace Desktopcafe.Agent.Services;

public class ScreenLockService
{
    public bool IsLocked { get; private set; } = true;

    public event Action? LockRequested;
    public event Action? UnlockRequested;

    public void Lock()
    {
        try
        {
            IsLocked = true;
            LockRequested?.Invoke();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to execute lock operation");
        }
    }

    public void Unlock()
    {
        try
        {
            IsLocked = false;
            UnlockRequested?.Invoke();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to execute unlock operation");
        }
    }
}
