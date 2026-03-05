using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktopcafe.Core.DTOs;
using Desktopcafe.Core.Enums;
using Desktopcafe.Core.Interfaces;

namespace Desktopcafe.Server.ViewModels;

public partial class TimerViewModel : ObservableObject
{
    private readonly ISessionService _sessionService;

    public TimerViewModel(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public ObservableCollection<SessionViewModel> ActiveSessions { get; } = new();

    [RelayCommand]
    private async Task StartSessionAsync()
    {
        // Session creation is handled via dialog; this triggers refresh after creation
        await LoadActiveSessionsAsync();
    }

    [RelayCommand]
    private async Task PauseSessionAsync(int sessionId)
    {
        try
        {
            await _sessionService.PauseSessionAsync(sessionId);
            await LoadActiveSessionsAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    [RelayCommand]
    private async Task ResumeAsync(int sessionId)
    {
        try
        {
            await _sessionService.ResumeSessionAsync(sessionId);
            await LoadActiveSessionsAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    [RelayCommand]
    private async Task EndSessionAsync(int sessionId)
    {
        try
        {
            await _sessionService.EndSessionAsync(sessionId);
            await LoadActiveSessionsAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    [RelayCommand]
    private async Task ExtendAsync(int sessionId)
    {
        try
        {
            await _sessionService.ExtendSessionAsync(new ExtendSessionDto(sessionId, 30));
            await LoadActiveSessionsAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    private async Task LoadActiveSessionsAsync()
    {
        var sessions = await _sessionService.GetActiveSessionsAsync();

        ActiveSessions.Clear();
        foreach (var s in sessions)
        {
            var totalMinutes = s.PlannedEnd.HasValue
                ? (s.PlannedEnd.Value - s.StartTime).TotalMinutes
                : 1.0;
            var progress = totalMinutes > 0
                ? Math.Clamp(1.0 - s.TimeRemaining.TotalMinutes / totalMinutes, 0, 1) * 100
                : 0;

            ActiveSessions.Add(new SessionViewModel
            {
                Id = s.Id,
                ComputerName = s.ComputerName,
                ClientName = s.ClientName ?? "Walk-in",
                TimeRemaining = s.TimeRemaining,
                Progress = progress,
                StatusColor = s.IsPaused ? "#FF9800" : progress > 80 ? "#F44336" : "#4CAF50",
                TotalCharge = s.TotalCharge,
                SessionType = s.SessionType
            });
        }
    }

    public partial class SessionViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _computerName = string.Empty;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private TimeSpan _timeRemaining;

        [ObservableProperty]
        private double _progress;

        [ObservableProperty]
        private string _statusColor = "#4CAF50";

        [ObservableProperty]
        private decimal _totalCharge;

        [ObservableProperty]
        private SessionType _sessionType;
    }
}
