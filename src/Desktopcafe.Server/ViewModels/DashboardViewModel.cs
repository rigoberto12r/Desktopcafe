using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktopcafe.Core.DTOs;
using Desktopcafe.Core.Interfaces;

namespace Desktopcafe.Server.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ICacheService _cacheService;

    public DashboardViewModel(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    [ObservableProperty]
    private decimal _totalRevenue;

    [ObservableProperty]
    private int _activeSessions;

    [ObservableProperty]
    private int _availablePCs;

    [ObservableProperty]
    private int _totalPCs;

    [ObservableProperty]
    private int _printJobsToday;

    [ObservableProperty]
    private int _salesToday;

    public ObservableCollection<ActivityItemDto> RecentActivity { get; } = new();

    public ObservableCollection<AlertItemDto> Alerts { get; } = new();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            var dashboard = await _cacheService.GetDashboardAsync();

            TotalRevenue = dashboard.TotalRevenue;
            ActiveSessions = dashboard.ActiveSessions;
            AvailablePCs = dashboard.AvailablePCs;
            TotalPCs = dashboard.TotalPCs;
            PrintJobsToday = dashboard.PrintJobsToday;
            SalesToday = dashboard.SalesToday;

            RecentActivity.Clear();
            foreach (var item in dashboard.RecentActivity)
            {
                RecentActivity.Add(item);
            }

            Alerts.Clear();
            foreach (var item in dashboard.Alerts)
            {
                Alerts.Add(item);
            }
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }
}
