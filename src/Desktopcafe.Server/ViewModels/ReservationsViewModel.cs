using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktopcafe.Core.Enums;
using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Server.ViewModels;

public partial class ReservationsViewModel : ObservableObject
{
    private readonly AppDbContext _db;

    public ReservationsViewModel(AppDbContext db)
    {
        _db = db;
        _selectedDate = DateTime.Today;
    }

    public ObservableCollection<Reservation> Reservations { get; } = new();

    [ObservableProperty]
    private DateTime _selectedDate;

    partial void OnSelectedDateChanged(DateTime value) => _ = LoadReservationsAsync();

    [RelayCommand]
    private async Task AddAsync()
    {
        // Dialog-driven: view layer handles reservation creation
        await LoadReservationsAsync();
    }

    [RelayCommand]
    private async Task CancelAsync(int reservationId)
    {
        try
        {
            var reservation = await _db.Reservations.FindAsync(reservationId);
            if (reservation is null) return;

            reservation.Status = ReservationStatus.Cancelled;
            _db.Reservations.Update(reservation);
            await _db.SaveChangesAsync();
            await LoadReservationsAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    private async Task LoadReservationsAsync()
    {
        try
        {
            var startOfDay = SelectedDate.Date;
            var endOfDay = startOfDay.AddDays(1);

            var reservations = await _db.Reservations
                .Include(r => r.Computer)
                .Include(r => r.Client)
                .Where(r => r.StartTime >= startOfDay && r.StartTime < endOfDay)
                .OrderBy(r => r.StartTime)
                .ToListAsync();

            Reservations.Clear();
            foreach (var reservation in reservations)
            {
                Reservations.Add(reservation);
            }
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }
}
