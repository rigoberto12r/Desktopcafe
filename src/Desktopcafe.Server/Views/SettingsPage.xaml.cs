using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Serilog;

namespace Desktopcafe.Server.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            using var db = new AppDbContext();
            var configs = await db.AppConfigs.ToListAsync();

            CafeNameBox.Text = GetConfig(configs, "CafeName", "Mi Cafe");
            LockScreenMessageBox.Text = GetConfig(configs, "LockScreenMessage", "Bienvenido");
            PrintPriceBWBox.Value = double.TryParse(GetConfig(configs, "PrintPriceBW", "1.00"), out var bw) ? bw : 1.00;
            PrintPriceColorBox.Value = double.TryParse(GetConfig(configs, "PrintPriceColor", "3.00"), out var color) ? color : 3.00;
            WarningMinutes1Box.Value = double.TryParse(GetConfig(configs, "WarningMinutes1", "15"), out var w1) ? w1 : 15;
            WarningMinutes2Box.Value = double.TryParse(GetConfig(configs, "WarningMinutes2", "10"), out var w2) ? w2 : 10;
            WarningMinutes3Box.Value = double.TryParse(GetConfig(configs, "WarningMinutes3", "5"), out var w3) ? w3 : 5;
            AutoBackupToggle.IsOn = GetConfig(configs, "AutoBackup", "false").Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load settings");
        }
    }

    private static string GetConfig(List<AppConfig> configs, string key, string defaultValue)
    {
        return configs.FirstOrDefault(c => c.Key == key)?.Value ?? defaultValue;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var db = new AppDbContext();
            var configs = await db.AppConfigs.ToListAsync();

            await SetConfigAsync(db, configs, "CafeName", CafeNameBox.Text?.Trim() ?? "");
            await SetConfigAsync(db, configs, "LockScreenMessage", LockScreenMessageBox.Text?.Trim() ?? "");
            await SetConfigAsync(db, configs, "PrintPriceBW", PrintPriceBWBox.Value.ToString("F2"));
            await SetConfigAsync(db, configs, "PrintPriceColor", PrintPriceColorBox.Value.ToString("F2"));
            await SetConfigAsync(db, configs, "WarningMinutes1", ((int)WarningMinutes1Box.Value).ToString());
            await SetConfigAsync(db, configs, "WarningMinutes2", ((int)WarningMinutes2Box.Value).ToString());
            await SetConfigAsync(db, configs, "WarningMinutes3", ((int)WarningMinutes3Box.Value).ToString());
            await SetConfigAsync(db, configs, "AutoBackup", AutoBackupToggle.IsOn ? "true" : "false");

            await db.SaveChangesAsync();

            await new ContentDialog
            {
                Title = "Configuracion",
                Content = "Configuracion guardada exitosamente.",
                CloseButtonText = "Aceptar",
                XamlRoot = XamlRoot
            }.ShowAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save settings");
            await new ContentDialog
            {
                Title = "Error",
                Content = "No se pudo guardar la configuracion.",
                CloseButtonText = "Aceptar",
                XamlRoot = XamlRoot
            }.ShowAsync();
        }
    }

    private static async Task SetConfigAsync(AppDbContext db, List<AppConfig> configs, string key, string value)
    {
        var existing = configs.FirstOrDefault(c => c.Key == key);
        if (existing != null)
        {
            existing.Value = value;
            db.Entry(existing).State = EntityState.Modified;
        }
        else
        {
            await db.AppConfigs.AddAsync(new AppConfig { Key = key, Value = value });
        }
    }
}
