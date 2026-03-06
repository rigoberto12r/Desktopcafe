using Desktopcafe.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Serilog;

namespace Desktopcafe.Server.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await RefreshDashboardAsync();
    }

    private async Task RefreshDashboardAsync()
    {
        try
        {
            var cache = App.Services.GetRequiredService<ICacheService>();
            var dashboard = await cache.GetDashboardAsync();

            RevenueText.Text = $"${dashboard.TotalRevenue:F2}";
            SessionsText.Text = dashboard.ActiveSessions.ToString();
            PCsText.Text = $"{dashboard.TotalPCs - dashboard.AvailablePCs}/{dashboard.TotalPCs}";
            PrintsText.Text = dashboard.PrintJobsToday.ToString();
            ActivityList.ItemsSource = dashboard.RecentActivity;
            AlertsList.ItemsSource = dashboard.Alerts;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load dashboard data");
        }
    }
}
