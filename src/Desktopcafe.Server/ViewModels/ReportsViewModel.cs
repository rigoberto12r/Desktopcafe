using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktopcafe.Core.DTOs;
using Desktopcafe.Core.Interfaces;

namespace Desktopcafe.Server.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly IReportService _reportService;

    public ReportsViewModel(IReportService reportService)
    {
        _reportService = reportService;
        _selectedDate = DateTime.Today;
    }

    [ObservableProperty]
    private DateTime _selectedDate;

    [ObservableProperty]
    private DailyReportDto? _report;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task GenerateReportAsync()
    {
        IsLoading = true;

        try
        {
            Report = await _reportService.GetDailyReportAsync(SelectedDate);
        }
        catch (Exception)
        {
            Report = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        if (Report is null) return;

        IsLoading = true;

        try
        {
            var bytes = await _reportService.ExportToPdfAsync(Report);
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"Report_{SelectedDate:yyyy-MM-dd}.pdf");
            await File.WriteAllBytesAsync(path, bytes);
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        if (Report is null) return;

        IsLoading = true;

        try
        {
            var bytes = await _reportService.ExportToExcelAsync(new List<DailyReportDto> { Report });
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"Report_{SelectedDate:yyyy-MM-dd}.xlsx");
            await File.WriteAllBytesAsync(path, bytes);
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
        finally
        {
            IsLoading = false;
        }
    }
}
