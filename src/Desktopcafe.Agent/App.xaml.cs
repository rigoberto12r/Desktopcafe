using Desktopcafe.Agent.Services;
using Desktopcafe.Agent.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Serilog;

namespace Desktopcafe.Agent;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static Window MainWindow { get; private set; } = null!;

    public App()
    {
        InitializeComponent();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Desktopcafe.Agent", "logs", "agent-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .CreateLogger();

        Services = ConfigureServices();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<SignalRClientService>();
        services.AddSingleton<ScreenLockService>();
        services.AddSingleton<HardwareInfoService>();
        services.AddSingleton<ProcessMonitorService>();
        services.AddSingleton<GameLauncherService>();

        services.AddTransient<LockScreenViewModel>();
        services.AddTransient<SessionOverlayViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();

        // Start SignalR connection
        _ = Task.Run(async () =>
        {
            try
            {
                var signalR = Services.GetRequiredService<SignalRClientService>();
                await signalR.ConnectAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to connect to server");
            }
        });
    }
}
