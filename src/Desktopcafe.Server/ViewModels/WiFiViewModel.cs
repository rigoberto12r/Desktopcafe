using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Server.ViewModels;

public partial class WiFiViewModel : ObservableObject
{
    private readonly AppDbContext _db;

    public WiFiViewModel(AppDbContext db)
    {
        _db = db;
    }

    public ObservableCollection<WiFiVoucher> Vouchers { get; } = new();

    [ObservableProperty]
    private int _newDurationMinutes = 60;

    [RelayCommand]
    private async Task GenerateVoucherAsync()
    {
        try
        {
            var voucher = new WiFiVoucher
            {
                Code = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
                DurationMinutes = NewDurationMinutes,
                IsUsed = false,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(1)
            };

            _db.WiFiVouchers.Add(voucher);
            await _db.SaveChangesAsync();
            await RefreshAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            var vouchers = await _db.WiFiVouchers
                .OrderByDescending(v => v.CreatedAt)
                .Take(100)
                .ToListAsync();

            Vouchers.Clear();
            foreach (var voucher in vouchers)
            {
                Vouchers.Add(voucher);
            }
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }
}
