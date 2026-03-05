using Desktopcafe.Core.DTOs;

namespace Desktopcafe.Shared.Hubs;

public interface IAgentHub
{
    Task LockScreen();
    Task UnlockScreen(SessionDto session);
    Task UpdateTimer(TimeSpan remaining, double progress);
    Task ShowWarning(string message, int minutesLeft);
    Task ShowMessage(string title, string message);
    Task Shutdown();
    Task Restart();
    Task RequestScreenshot();
    Task UpdateGameCatalog(List<GameDto> games);
    Task UpdateConfig(AgentConfigDto config);
    Task BatchUpdateTimers(List<TimerUpdateDto> updates);
}
