using Desktopcafe.Core.DTOs;

namespace Desktopcafe.Shared.Hubs;

public interface IServerHub
{
    Task Register(ComputerRegistrationDto info);
    Task Heartbeat(HeartbeatDto status);
    Task PrintJobDetected(PrintJobDto job);
    Task SessionRequest(SessionRequestDto request);
    Task AlertAdmin(AlertDto alert);
    Task SendScreenshot(byte[] imageData);
    Task GameLaunched(int gameId);
}
