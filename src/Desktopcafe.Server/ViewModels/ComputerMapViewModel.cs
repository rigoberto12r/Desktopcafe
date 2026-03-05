using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktopcafe.Core.Enums;
using Desktopcafe.Core.Interfaces;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Server.ViewModels;

public partial class ComputerMapViewModel : ObservableObject
{
    private readonly IComputerService _computerService;

    public ComputerMapViewModel(IComputerService computerService)
    {
        _computerService = computerService;
    }

    public ObservableCollection<ComputerCardViewModel> Computers { get; } = new();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            var computers = await _computerService.GetAllComputersAsync();

            Computers.Clear();
            foreach (var c in computers)
            {
                var remaining = c.ActiveSession?.TimeRemaining ?? TimeSpan.Zero;
                var totalMinutes = c.ActiveSession is not null && c.ActiveSession.PlannedEnd.HasValue
                    ? (c.ActiveSession.PlannedEnd.Value - c.ActiveSession.StartTime).TotalMinutes
                    : 1.0;
                var progress = totalMinutes > 0
                    ? Math.Clamp(1.0 - remaining.TotalMinutes / totalMinutes, 0, 1) * 100
                    : 0;

                Computers.Add(new ComputerCardViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Status = c.Status,
                    IsOnline = c.IsOnline,
                    TimeRemaining = remaining,
                    Progress = progress,
                    ClientName = c.ActiveSession?.ClientName ?? string.Empty,
                    StatusColor = c.Status switch
                    {
                        ComputerStatus.Available => "#4CAF50",
                        ComputerStatus.InUse => "#2196F3",
                        ComputerStatus.Maintenance => "#FF9800",
                        ComputerStatus.Offline => "#9E9E9E",
                        _ => "#9E9E9E"
                    }
                });
            }
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    [RelayCommand]
    private async Task SelectComputerAsync(int id)
    {
        var computer = await _computerService.GetComputerAsync(id);
        // Selection handling delegated to the view layer
    }

    public partial class ComputerCardViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private ComputerStatus _status;

        [ObservableProperty]
        private bool _isOnline;

        [ObservableProperty]
        private TimeSpan _timeRemaining;

        [ObservableProperty]
        private double _progress;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _statusColor = "#9E9E9E";
    }
}
