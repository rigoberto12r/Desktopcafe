using Desktopcafe.Core.DTOs;

namespace Desktopcafe.Core.Interfaces;

public interface IReportService
{
    Task<DashboardDto> GetDashboardAsync();
    Task<DailyReportDto> GetDailyReportAsync(DateTime date);
    Task<List<DailyReportDto>> GetRangeReportAsync(DateTime from, DateTime to);
    Task<byte[]> ExportToPdfAsync(DailyReportDto report);
    Task<byte[]> ExportToExcelAsync(List<DailyReportDto> reports);
}
