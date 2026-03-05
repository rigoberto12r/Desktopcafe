namespace Desktopcafe.Core.DTOs;

public record DailyReportDto
{
    public DateTime Date { get; init; }
    public decimal SessionRevenue { get; init; }
    public decimal SalesRevenue { get; init; }
    public decimal PrintRevenue { get; init; }
    public decimal TotalRevenue => SessionRevenue + SalesRevenue + PrintRevenue;
    public int TotalSessions { get; init; }
    public int TotalSales { get; init; }
    public int TotalPrintJobs { get; init; }
    public List<HourlyUsageDto> HourlyUsage { get; init; } = new();
    public List<TopProductDto> TopProducts { get; init; } = new();
    public List<ComputerUsageDto> ComputerUsage { get; init; } = new();
}

public record HourlyUsageDto(int Hour, int SessionCount, double AvgMinutes);

public record TopProductDto(string Name, int TotalQuantity, decimal TotalRevenue);

public record ComputerUsageDto(string ComputerName, int SessionCount, double TotalHours);

public record RevenueBreakdownDto(decimal SessionRevenue, decimal SalesRevenue, decimal PrintRevenue);
