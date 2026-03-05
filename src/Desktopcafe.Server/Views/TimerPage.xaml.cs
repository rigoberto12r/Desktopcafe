using Desktopcafe.Core.Enums;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Desktopcafe.Server.Views;

public sealed partial class TimerPage : Page
{
    private readonly PeriodicTimer _refreshTimer = new(TimeSpan.FromSeconds(1));
    private CancellationTokenSource _cts = new();

    public TimerPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _cts = new CancellationTokenSource();
        _ = StartTimerLoopAsync(_cts.Token);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _cts.Cancel();
    }

    private async Task StartTimerLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await RefreshSessionsAsync();
            try { await _refreshTimer.WaitForNextTickAsync(ct); }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task RefreshSessionsAsync()
    {
        try
        {
            using var db = new AppDbContext();
            var sessions = await db.Sessions
                .Include(s => s.Computer)
                .Include(s => s.Client)
                .Where(s => s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused)
                .OrderBy(s => s.PlannedEnd)
                .ToListAsync();

            var items = sessions.Select(s =>
            {
                var remaining = s.PlannedEnd.HasValue ? s.PlannedEnd.Value - DateTime.Now : TimeSpan.MaxValue;
                if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
                var total = s.PlannedEnd.HasValue ? (s.PlannedEnd.Value - s.StartTime).TotalSeconds : 1;
                var progress = total > 0 ? (1.0 - remaining.TotalSeconds / total) * 100 : 0;

                return new
                {
                    s.Id,
                    ComputerName = s.Computer.Name,
                    ClientName = s.Client?.Name ?? "Sin cliente",
                    TimeRemainingText = s.SessionType == SessionType.Free
                        ? (DateTime.Now - s.StartTime).ToString(@"h\:mm\:ss")
                        : remaining.ToString(remaining.TotalHours >= 1 ? @"h\:mm\:ss" : @"mm\:ss"),
                    Progress = Math.Clamp(progress, 0, 100),
                    StatusColor = remaining.TotalMinutes <= 1 ? App.Current.Resources["StatusErrorBrush"]
                        : remaining.TotalMinutes <= 5 ? App.Current.Resources["StatusWarningBrush"]
                        : App.Current.Resources["StatusAvailableBrush"],
                    SessionTypeText = s.SessionType switch
                    {
                        SessionType.Prepaid => "Prepago",
                        SessionType.Postpaid => "Postpago",
                        SessionType.Free => "Libre",
                        _ => ""
                    },
                    TotalChargeText = $"${s.TotalCharge:F2}",
                    s.IsPaused
                };
            }).ToList();

            DispatcherQueue.TryEnqueue(() => SessionList.ItemsSource = items);
        }
        catch { }
    }

    private async void NewSession_Click(object sender, RoutedEventArgs e)
    {
        // Show new session dialog
        using var db = new AppDbContext();
        var availablePCs = await db.Computers
            .Where(c => c.Status == ComputerStatus.Available)
            .ToListAsync();

        if (!availablePCs.Any())
        {
            await new ContentDialog
            {
                Title = "Sin PCs disponibles",
                Content = "No hay computadoras disponibles en este momento.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            }.ShowAsync();
            return;
        }

        var pcCombo = new ComboBox { Header = "Computadora", ItemsSource = availablePCs.Select(p => p.Name).ToList(), Width = 300 };
        var typeCombo = new ComboBox { Header = "Tipo de Sesion", ItemsSource = new[] { "Prepago", "Postpago", "Libre" }, SelectedIndex = 0, Width = 300 };
        var durationBox = new NumberBox { Header = "Duracion (minutos)", Value = 60, Minimum = 15, Maximum = 480, Width = 300 };
        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(pcCombo);
        panel.Children.Add(typeCombo);
        panel.Children.Add(durationBox);

        var dialog = new ContentDialog
        {
            Title = "Nueva Sesion",
            Content = panel,
            PrimaryButtonText = "Iniciar",
            CloseButtonText = "Cancelar",
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary && pcCombo.SelectedIndex >= 0)
        {
            var pc = availablePCs[pcCombo.SelectedIndex];
            var sessionType = (SessionType)typeCombo.SelectedIndex;
            var duration = (int)durationBox.Value;
            var rate = await db.RateConfigs.FirstOrDefaultAsync(r => r.IsDefault);
            var ratePerHour = rate?.PricePerHour ?? 20m;

            var session = new Core.Models.Session
            {
                ComputerId = pc.Id,
                EmployeeId = App.CurrentEmployeeId,
                SessionType = sessionType,
                RatePerHour = ratePerHour,
                StartTime = DateTime.Now,
                PlannedEnd = sessionType != SessionType.Free ? DateTime.Now.AddMinutes(duration) : null,
                TotalCharge = sessionType == SessionType.Prepaid ? ratePerHour * duration / 60m : 0,
                Status = SessionStatus.Active
            };

            db.Sessions.Add(session);
            pc.Status = ComputerStatus.InUse;
            db.Entry(pc).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
    }

    private async void Extend_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int sessionId)
        {
            var box = new NumberBox { Header = "Minutos adicionales", Value = 30, Minimum = 5, Maximum = 240 };
            var dialog = new ContentDialog
            {
                Title = "Extender Sesion",
                Content = box,
                PrimaryButtonText = "Extender",
                CloseButtonText = "Cancelar",
                XamlRoot = XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                using var db = new AppDbContext();
                var session = await db.Sessions.FindAsync(sessionId);
                if (session?.PlannedEnd != null)
                {
                    session.PlannedEnd = session.PlannedEnd.Value.AddMinutes((int)box.Value);
                    session.TotalCharge += session.RatePerHour * (decimal)box.Value / 60m;
                    db.Entry(session).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
            }
        }
    }

    private async void PauseResume_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int sessionId)
        {
            using var db = new AppDbContext();
            var session = await db.Sessions.FindAsync(sessionId);
            if (session == null) return;

            if (session.IsPaused)
            {
                var pausedTime = DateTime.Now - (session.PausedAt ?? DateTime.Now);
                session.PausedDuration += pausedTime;
                if (session.PlannedEnd.HasValue)
                    session.PlannedEnd = session.PlannedEnd.Value.Add(pausedTime);
                session.IsPaused = false;
                session.PausedAt = null;
                session.Status = SessionStatus.Active;
            }
            else
            {
                session.IsPaused = true;
                session.PausedAt = DateTime.Now;
                session.Status = SessionStatus.Paused;
            }

            db.Entry(session).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
    }

    private async void EndSession_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int sessionId)
        {
            using var db = new AppDbContext();
            var session = await db.Sessions.Include(s => s.Computer).FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null) return;

            session.EndTime = DateTime.Now;
            session.Status = SessionStatus.Completed;

            if (session.SessionType == SessionType.Postpaid)
            {
                var duration = session.EndTime.Value - session.StartTime - session.PausedDuration;
                session.TotalCharge = session.RatePerHour * (decimal)duration.TotalHours;
            }

            session.Computer.Status = ComputerStatus.Available;
            db.Entry(session).State = EntityState.Modified;
            db.Entry(session.Computer).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
    }
}
