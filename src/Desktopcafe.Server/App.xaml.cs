using Desktopcafe.Core.Interfaces;
using Desktopcafe.Data;
using Desktopcafe.Data.Repositories;
using Desktopcafe.Data.Seeding;
using Desktopcafe.Server.Services;
using Desktopcafe.Server.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Serilog;

namespace Desktopcafe.Server;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static Window MainWindow { get; private set; } = null!;
    public static int CurrentEmployeeId { get; set; }

    public App()
    {
        InitializeComponent();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Desktopcafe", "logs", "server-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        Services = ConfigureServices();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Data
        services.AddDbContext<AppDbContext>(ServiceLifetime.Transient);
        services.AddTransient<SessionRepository>();
        services.AddTransient<SaleRepository>();
        services.AddTransient<ReportRepository>();

        // Cache
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1024;
            options.CompactionPercentage = 0.25;
        });

        // Services
        services.AddSingleton<NavigationService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<SessionTimerService>();
        services.AddSingleton<BillingService>();
        services.AddSingleton<SignalRHostService>();
        services.AddSingleton<BackupService>();

        // ViewModels
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ComputerMapViewModel>();
        services.AddTransient<TimerViewModel>();
        services.AddTransient<POSViewModel>();
        services.AddTransient<ClientsViewModel>();
        services.AddTransient<PrinterViewModel>();
        services.AddTransient<InventoryViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<EmployeesViewModel>();
        services.AddTransient<ReservationsViewModel>();
        services.AddTransient<WiFiViewModel>();
        services.AddTransient<GamingViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<LoginViewModel>();

        return services.BuildServiceProvider();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();

        // Seed database and apply pragmas
        try
        {
            using var db = new AppDbContext();
            await InitialDataSeeder.SeedAsync(db);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during database initialization");
        }

        // Preload cache in background
        _ = Task.Run(async () =>
        {
            try
            {
                var cache = Services.GetRequiredService<ICacheService>();
                await cache.GetRatesAsync();
                await cache.GetActiveProductsAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error preloading cache");
            }
        });
    }
}
