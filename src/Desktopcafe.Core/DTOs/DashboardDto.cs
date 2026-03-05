namespace Desktopcafe.Core.DTOs;

public record DashboardDto
{
    public decimal TotalRevenue { get; init; }
    public int ActiveSessions { get; init; }
    public int AvailablePCs { get; init; }
    public int TotalPCs { get; init; }
    public int PrintJobsToday { get; init; }
    public int SalesToday { get; init; }
    public decimal RevenueChange { get; init; }
    public List<ActivityItemDto> RecentActivity { get; init; } = new();
    public List<AlertItemDto> Alerts { get; init; } = new();
}

public record ActivityItemDto(
    DateTime Timestamp,
    string Description,
    string Type);

public record AlertItemDto(
    string Message,
    string Severity,
    int? ComputerId);
