using Desktopcafe.Core.DTOs;
using Desktopcafe.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Desktopcafe.Data.Repositories;

public class ReportRepository
{
    private readonly AppDbContext _db;

    public ReportRepository(AppDbContext db) => _db = db;

    public async Task<DashboardDto> GetDashboardAsync()
    {
        try
        {
            var today = DateTime.Today;

            var activeSessionsTask = _db.Sessions.CountAsync(s => s.Status == SessionStatus.Active);
            var availablePCsTask = _db.Computers.CountAsync(c => c.Status == ComputerStatus.Available);
            var totalPCsTask = _db.Computers.CountAsync();
            var printJobsTask = _db.PrintJobs.CountAsync(p => p.CreatedAt >= today);
            var salesTodayTask = _db.Sales.CountAsync(s => s.CreatedAt >= today);

            var sessionRevTask = _db.Sessions
                .Where(s => s.CreatedAt >= today)
                .SumAsync(s => (decimal?)s.TotalCharge ?? 0);
            var saleRevTask = _db.Sales
                .Where(s => s.CreatedAt >= today)
                .SumAsync(s => (decimal?)s.Total ?? 0);
            var printRevTask = _db.PrintJobs
                .Where(p => p.CreatedAt >= today && p.Status == PrintJobStatus.Printed)
                .SumAsync(p => (decimal?)p.Cost ?? 0);

            await Task.WhenAll(
                activeSessionsTask, availablePCsTask, totalPCsTask,
                printJobsTask, salesTodayTask, sessionRevTask, saleRevTask, printRevTask);

            return new DashboardDto
            {
                ActiveSessions = await activeSessionsTask,
                AvailablePCs = await availablePCsTask,
                TotalPCs = await totalPCsTask,
                PrintJobsToday = await printJobsTask,
                SalesToday = await salesTodayTask,
                TotalRevenue = await sessionRevTask + await saleRevTask + await printRevTask
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get dashboard data");
            throw;
        }
    }

    public async Task<DailyReportDto> GetDailyReportAsync(DateTime date)
    {
        try
        {
            var nextDay = date.AddDays(1);

            var sessionRev = await _db.Sessions
                .Where(s => s.CreatedAt >= date && s.CreatedAt < nextDay)
                .SumAsync(s => (decimal?)s.TotalCharge ?? 0);

            var salesRev = await _db.Sales
                .Where(s => s.CreatedAt >= date && s.CreatedAt < nextDay)
                .SumAsync(s => (decimal?)s.Total ?? 0);

            var printRev = await _db.PrintJobs
                .Where(p => p.CreatedAt >= date && p.CreatedAt < nextDay && p.Status == PrintJobStatus.Printed)
                .SumAsync(p => (decimal?)p.Cost ?? 0);

            var hourlyUsage = await _db.Sessions
                .Where(s => s.CreatedAt >= date && s.CreatedAt < nextDay)
                .GroupBy(s => s.StartTime.Hour)
                .Select(g => new HourlyUsageDto(g.Key, g.Count(), g.Average(s => EF.Functions.DateDiffMinute(s.StartTime, s.EndTime ?? DateTime.Now))))
                .OrderBy(h => h.Hour)
                .ToListAsync();

            var topProducts = await _db.SaleItems
                .Include(si => si.Sale)
                .Include(si => si.Product)
                .Where(si => si.Sale.CreatedAt >= date && si.Sale.CreatedAt < nextDay)
                .GroupBy(si => si.Product.Name)
                .Select(g => new TopProductDto(g.Key, g.Sum(x => x.Quantity), g.Sum(x => x.Subtotal)))
                .OrderByDescending(p => p.TotalRevenue)
                .Take(10)
                .ToListAsync();

            var computerUsage = await _db.Sessions
                .Include(s => s.Computer)
                .Where(s => s.CreatedAt >= date && s.CreatedAt < nextDay)
                .GroupBy(s => s.Computer.Name)
                .Select(g => new ComputerUsageDto(
                    g.Key,
                    g.Count(),
                    g.Sum(s => EF.Functions.DateDiffMinute(s.StartTime, s.EndTime ?? DateTime.Now)) / 60.0))
                .OrderByDescending(c => c.TotalHours)
                .ToListAsync();

            return new DailyReportDto
            {
                Date = date,
                SessionRevenue = sessionRev,
                SalesRevenue = salesRev,
                PrintRevenue = printRev,
                TotalSessions = await _db.Sessions.CountAsync(s => s.CreatedAt >= date && s.CreatedAt < nextDay),
                TotalSales = await _db.Sales.CountAsync(s => s.CreatedAt >= date && s.CreatedAt < nextDay),
                TotalPrintJobs = await _db.PrintJobs.CountAsync(p => p.CreatedAt >= date && p.CreatedAt < nextDay),
                HourlyUsage = hourlyUsage,
                TopProducts = topProducts,
                ComputerUsage = computerUsage
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get daily report for {Date}", date);
            throw;
        }
    }
}
