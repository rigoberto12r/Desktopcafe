using Desktopcafe.Agent.Services;
using Desktopcafe.Core.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Desktopcafe.Agent.Views;

public sealed partial class SessionOverlay : UserControl
{
    private bool _isExpanded;
    private SessionDto? _currentSession;

    public SessionOverlay()
    {
        InitializeComponent();
    }

    public void UpdateSession(SessionDto session)
    {
        _currentSession = session;
        DispatcherQueue.TryEnqueue(() =>
        {
            ComputerNameText.Text = $"Desktopcafe - {session.ComputerName}";
            var timeText = session.TimeRemaining.TotalHours >= 1
                ? session.TimeRemaining.ToString(@"h\:mm\:ss")
                : session.TimeRemaining.ToString(@"mm\:ss");

            TimeCompactText.Text = timeText;
            TimeExpandedText.Text = timeText;
            ChargeCompactText.Text = $"${session.TotalCharge:F2}";
            ChargeExpandedText.Text = $"${session.TotalCharge:F2}";
            ClientNameText.Text = session.ClientName ?? "Sin cliente";
            SessionProgress.Value = (1 - session.TimeRemaining.TotalSeconds /
                Math.Max(1, (session.PlannedEnd - session.StartTime)?.TotalSeconds ?? 1)) * 100;
        });
    }

    public void UpdateTimer(TimeSpan remaining, double progress)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            var timeText = remaining.TotalHours >= 1
                ? remaining.ToString(@"h\:mm\:ss")
                : remaining.ToString(@"mm\:ss");
            TimeCompactText.Text = timeText;
            TimeExpandedText.Text = timeText;
            SessionProgress.Value = progress * 100;
        });
    }

    private void ToggleExpand_Click(object sender, RoutedEventArgs e)
    {
        _isExpanded = !_isExpanded;
        CompactView.Visibility = _isExpanded ? Visibility.Collapsed : Visibility.Visible;
        ExpandedView.Visibility = _isExpanded ? Visibility.Visible : Visibility.Collapsed;
        ExpandIcon.Glyph = _isExpanded ? "\uE70E" : "\uE70D";
    }

    private async void RequestExtend_Click(object sender, RoutedEventArgs e)
    {
        var signalR = App.Services.GetRequiredService<SignalRClientService>();
        await signalR.RequestExtensionAsync();
    }

    private async void CallAdmin_Click(object sender, RoutedEventArgs e)
    {
        var signalR = App.Services.GetRequiredService<SignalRClientService>();
        await signalR.AlertAdminAsync("Solicitud de asistencia", "El usuario solicita ayuda");
    }
}
