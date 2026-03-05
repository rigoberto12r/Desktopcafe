using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Desktopcafe.Server.Views;

public sealed partial class WiFiPage : Page
{
    private static readonly string VoucherChars = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    public WiFiPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadVouchersAsync();
    }

    private async Task LoadVouchersAsync()
    {
        using var db = new AppDbContext();
        var vouchers = await db.WiFiVouchers
            .OrderByDescending(v => v.CreatedAt)
            .Take(100)
            .ToListAsync();

        VoucherList.ItemsSource = vouchers.Select(v => new
        {
            v.Code,
            DurationText = $"{v.DurationMinutes} min",
            StatusText = v.IsUsed ? "Usado" : "Disponible",
            StatusColor = v.IsUsed ? "Gray" : "Green",
            CreatedText = v.CreatedAt.ToString("dd/MM/yyyy HH:mm")
        }).ToList();
    }

    private async void GenerateVoucher_Click(object sender, RoutedEventArgs e)
    {
        var duration = (int)DurationBox.Value;
        if (duration <= 0) return;

        var voucher = new WiFiVoucher
        {
            Code = GenerateVoucherCode(),
            DurationMinutes = duration,
            ExpiresAt = DateTime.Now.AddDays(7)
        };

        using var db = new AppDbContext();
        await db.WiFiVouchers.AddAsync(voucher);
        await db.SaveChangesAsync();
        await LoadVouchersAsync();
    }

    private static string GenerateVoucherCode()
    {
        var random = new Random();
        return new string(Enumerable.Range(0, 8).Select(_ => VoucherChars[random.Next(VoucherChars.Length)]).ToArray());
    }
}
