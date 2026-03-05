namespace Desktopcafe.Core.DTOs;

public record AgentConfigDto(
    string CafeName,
    string LockScreenMessage,
    bool ShowWiFiInfo,
    bool AllowSessionRequest,
    bool ShowGameBrowser,
    int WarningMinutes1,
    int WarningMinutes2,
    int WarningMinutes3);

public record TimerUpdateDto(
    string MacAddress,
    TimeSpan TimeRemaining,
    double Progress);

public record AlertDto(
    string MacAddress,
    string Title,
    string Message,
    string Severity);

public record SessionRequestDto(
    string MacAddress,
    string? ClientCode);
