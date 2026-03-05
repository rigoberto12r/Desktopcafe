using Desktopcafe.Core.DTOs;

namespace Desktopcafe.Core.Interfaces;

public interface ISessionService
{
    Task<SessionDto> StartSessionAsync(CreateSessionDto dto, int employeeId);
    Task<SessionDto> PauseSessionAsync(int sessionId);
    Task<SessionDto> ResumeSessionAsync(int sessionId);
    Task<SessionDto> ExtendSessionAsync(ExtendSessionDto dto);
    Task<SessionDto> EndSessionAsync(int sessionId);
    Task<List<SessionDto>> GetActiveSessionsAsync();
    Task<SessionDto?> GetActiveSessionForComputerAsync(int computerId);
    Task<List<SessionDto>> GetSessionHistoryAsync(DateTime from, DateTime to);
}
