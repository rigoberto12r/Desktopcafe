using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Desktopcafe.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Server.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppDbContext _db;
    private readonly BackupService _backupService;

    public SettingsViewModel(AppDbContext db, BackupService backupService)
    {
        _db = db;
        _backupService = backupService;
    }

    [ObservableProperty]
    private string _cafeName = string.Empty;

    [ObservableProperty]
    private decimal _printPriceBW;

    [ObservableProperty]
    private decimal _printPriceColor;

    [ObservableProperty]
    private int _warningMinutes1 = 10;

    [ObservableProperty]
    private int _warningMinutes2 = 5;

    [ObservableProperty]
    private int _warningMinutes3 = 1;

    [ObservableProperty]
    private string _lockScreenMessage = string.Empty;

    [ObservableProperty]
    private bool _autoBackupEnabled;

    [ObservableProperty]
    private int _backupIntervalHours = 24;

    public async Task LoadSettingsAsync()
    {
        try
        {
            var configs = await _db.AppConfigs.ToListAsync();
            var dict = configs.ToDictionary(c => c.Key, c => c.Value);

            CafeName = dict.GetValueOrDefault("CafeName", "Desktopcafe");
            PrintPriceBW = decimal.TryParse(dict.GetValueOrDefault("PrintPriceBW", "0.5"), out var bw) ? bw : 0.5m;
            PrintPriceColor = decimal.TryParse(dict.GetValueOrDefault("PrintPriceColor", "1.0"), out var color) ? color : 1.0m;
            WarningMinutes1 = int.TryParse(dict.GetValueOrDefault("WarningMinutes1", "10"), out var w1) ? w1 : 10;
            WarningMinutes2 = int.TryParse(dict.GetValueOrDefault("WarningMinutes2", "5"), out var w2) ? w2 : 5;
            WarningMinutes3 = int.TryParse(dict.GetValueOrDefault("WarningMinutes3", "1"), out var w3) ? w3 : 1;
            LockScreenMessage = dict.GetValueOrDefault("LockScreenMessage", "Please ask staff to start a session.");
            AutoBackupEnabled = bool.TryParse(dict.GetValueOrDefault("AutoBackupEnabled", "false"), out var ab) && ab;
            BackupIntervalHours = int.TryParse(dict.GetValueOrDefault("BackupIntervalHours", "24"), out var bi) ? bi : 24;
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            await SetConfigAsync("CafeName", CafeName);
            await SetConfigAsync("PrintPriceBW", PrintPriceBW.ToString());
            await SetConfigAsync("PrintPriceColor", PrintPriceColor.ToString());
            await SetConfigAsync("WarningMinutes1", WarningMinutes1.ToString());
            await SetConfigAsync("WarningMinutes2", WarningMinutes2.ToString());
            await SetConfigAsync("WarningMinutes3", WarningMinutes3.ToString());
            await SetConfigAsync("LockScreenMessage", LockScreenMessage);
            await SetConfigAsync("AutoBackupEnabled", AutoBackupEnabled.ToString());
            await SetConfigAsync("BackupIntervalHours", BackupIntervalHours.ToString());

            await _db.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    [RelayCommand]
    private async Task TestBackupAsync()
    {
        try
        {
            await _backupService.BackupNowAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    private async Task SetConfigAsync(string key, string value)
    {
        var config = await _db.AppConfigs.FirstOrDefaultAsync(c => c.Key == key);
        if (config is not null)
        {
            config.Value = value;
            _db.AppConfigs.Update(config);
        }
        else
        {
            _db.AppConfigs.Add(new AppConfig { Key = key, Value = value });
        }
    }
}
