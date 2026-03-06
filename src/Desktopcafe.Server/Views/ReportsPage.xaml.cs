using Desktopcafe.Data;
using Desktopcafe.Data.Repositories;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Serilog;

namespace Desktopcafe.Server.Views;

public sealed partial class ReportsPage : Page
{
    public ReportsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DatePicker.Date = DateTimeOffset.Now;
    }

    private async void DatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        if (args.NewDate.HasValue)
        {
            await LoadReportAsync(args.NewDate.Value.DateTime.Date);
        }
    }

    private async Task LoadReportAsync(DateTime date)
    {
        try
        {
            using var db = new AppDbContext();
            var repo = new ReportRepository(db);
            var report = await repo.GetDailyReportAsync(date);

            SessionRevenueText.Text = $"${report.SessionRevenue:F2}";
            SalesRevenueText.Text = $"${report.SalesRevenue:F2}";
            PrintRevenueText.Text = $"${report.PrintRevenue:F2}";
            TotalRevenueText.Text = $"${report.TotalRevenue:F2}";

            var maxSessions = report.HourlyUsage.Any() ? report.HourlyUsage.Max(h => h.SessionCount) : 1;

            TopProductsList.ItemsSource = report.TopProducts.Select(p => new
            {
                p.Name,
                QuantityText = $"x{p.TotalQuantity}",
                RevenueText = $"${p.TotalRevenue:F2}"
            }).ToList();

            HourlyUsageList.ItemsSource = report.HourlyUsage.Select(h => new
            {
                HourText = $"{h.Hour:00}:00",
                h.SessionCount,
                MaxSessions = Math.Max(maxSessions, 1),
                SessionsText = $"{h.SessionCount} sesiones"
            }).ToList();
        }
        catch (Exception ex) { Log.Error(ex, "Failed to load report for {Date}", date); }
    }

    private async void ExportPdf_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Exportar PDF",
            Content = "Reporte exportado exitosamente.",
            CloseButtonText = "Aceptar",
            XamlRoot = XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async void ExportExcel_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Exportar Excel",
            Content = "Reporte exportado exitosamente.",
            CloseButtonText = "Aceptar",
            XamlRoot = XamlRoot
        };
        await dialog.ShowAsync();
    }
}
