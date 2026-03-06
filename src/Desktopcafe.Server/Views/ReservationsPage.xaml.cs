using Desktopcafe.Core.Enums;
using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Serilog;

namespace Desktopcafe.Server.Views;

public sealed partial class ReservationsPage : Page
{
    public ReservationsPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadComputersAsync();
        await LoadReservationsAsync();
        ReservationDate.Date = DateTimeOffset.Now;
        ReservationTime.SelectedTime = DateTime.Now.TimeOfDay;
    }

    private async Task LoadComputersAsync()
    {
        try
        {
            using var db = new AppDbContext();
            var computers = await db.Computers.OrderBy(c => c.Name).ToListAsync();
            ComputerCombo.ItemsSource = computers;
            ComputerCombo.DisplayMemberPath = "Name";
            if (computers.Any()) ComputerCombo.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load computers for reservations");
        }
    }

    private async Task LoadReservationsAsync()
    {
        try
        {
            using var db = new AppDbContext();
            var reservations = await db.Reservations
                .Include(r => r.Computer)
                .Where(r => r.StartTime >= DateTime.Today)
                .OrderBy(r => r.StartTime)
                .ToListAsync();

            ReservationList.ItemsSource = reservations.Select(r => new
            {
                r.Id,
                ComputerName = r.Computer?.Name ?? "Desconocido",
                StartTimeText = r.StartTime.ToString("dd/MM/yyyy HH:mm"),
                DurationText = $"{r.DurationMinutes} min",
                StatusText = r.Status switch
                {
                    ReservationStatus.Pending => "Pendiente",
                    ReservationStatus.Confirmed => "Confirmada",
                    ReservationStatus.InProgress => "En Curso",
                    ReservationStatus.Completed => "Completada",
                    ReservationStatus.Cancelled => "Cancelada",
                    _ => r.Status.ToString()
                },
                StatusColor = r.Status switch
                {
                    ReservationStatus.Pending => "Orange",
                    ReservationStatus.Confirmed => "Green",
                    ReservationStatus.InProgress => "Blue",
                    ReservationStatus.Completed => "Gray",
                    ReservationStatus.Cancelled => "Red",
                    _ => "Gray"
                },
                CanCancel = r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed
                    ? Visibility.Visible : Visibility.Collapsed
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load reservations");
        }
    }

    private async void CreateReservation_Click(object sender, RoutedEventArgs e)
    {
        if (ComputerCombo.SelectedItem is not Computer computer) return;
        if (!ReservationDate.Date.HasValue) return;
        if (!ReservationTime.SelectedTime.HasValue) return;

        var date = ReservationDate.Date.Value.DateTime.Date;
        var time = ReservationTime.SelectedTime.Value;
        var startTime = date.Add(time);
        var duration = (int)DurationBox.Value;

        try
        {
            var reservation = new Reservation
            {
                ComputerId = computer.Id,
                StartTime = startTime,
                DurationMinutes = duration,
                Status = ReservationStatus.Pending
            };

            using var db = new AppDbContext();
            await db.Reservations.AddAsync(reservation);
            await db.SaveChangesAsync();
            await LoadReservationsAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create reservation for computer {ComputerId}", computer.Id);
            await new ContentDialog
            {
                Title = "Error",
                Content = "No se pudo crear la reservacion.",
                CloseButtonText = "Aceptar",
                XamlRoot = XamlRoot
            }.ShowAsync();
        }
    }

    private async void CancelReservation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int reservationId) return;

        try
        {
            using var db = new AppDbContext();
            var reservation = await db.Reservations.FindAsync(reservationId);
            if (reservation == null) return;

            reservation.Status = ReservationStatus.Cancelled;
            db.Entry(reservation).State = EntityState.Modified;
            await db.SaveChangesAsync();
            await LoadReservationsAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to cancel reservation {ReservationId}", reservationId);
        }
    }
}
