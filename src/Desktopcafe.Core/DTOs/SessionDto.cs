using Desktopcafe.Core.Enums;

namespace Desktopcafe.Core.DTOs;

public record SessionDto(
    int Id,
    int ComputerId,
    string ComputerName,
    int? ClientId,
    string? ClientName,
    DateTime StartTime,
    DateTime? PlannedEnd,
    SessionType SessionType,
    decimal RatePerHour,
    decimal TotalCharge,
    bool IsPaused,
    SessionStatus Status,
    TimeSpan TimeRemaining);

public record CreateSessionDto(
    int ComputerId,
    int? ClientId,
    SessionType SessionType,
    int? DurationMinutes,
    decimal RatePerHour);

public record ExtendSessionDto(
    int SessionId,
    int AdditionalMinutes);
