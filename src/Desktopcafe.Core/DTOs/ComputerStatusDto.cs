using Desktopcafe.Core.Enums;

namespace Desktopcafe.Core.DTOs;

public record ComputerStatusDto(
    int Id,
    string Name,
    ComputerStatus Status,
    bool IsOnline,
    bool IsGaming,
    string Zone,
    int PositionX,
    int PositionY,
    SessionDto? ActiveSession);

public record ComputerRegistrationDto(
    string Name,
    string IpAddress,
    string MacAddress,
    string? Specs);

public record HeartbeatDto(
    string MacAddress,
    double CpuUsage,
    double RamUsage,
    double DiskUsage);
