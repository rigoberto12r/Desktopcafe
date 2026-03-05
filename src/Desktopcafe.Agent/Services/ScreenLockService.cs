namespace Desktopcafe.Agent.Services;

public class ScreenLockService
{
    public bool IsLocked { get; private set; } = true;

    public event Action? LockRequested;
    public event Action? UnlockRequested;

    public void Lock()
    {
        IsLocked = true;
        LockRequested?.Invoke();
    }

    public void Unlock()
    {
        IsLocked = false;
        UnlockRequested?.Invoke();
    }
}
