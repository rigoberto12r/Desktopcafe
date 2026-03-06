using Desktopcafe.Agent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Serilog;

namespace Desktopcafe.Agent;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "Desktopcafe Agent";
        ExtendsContentIntoTitleBar = true;

        var screenLock = App.Services.GetRequiredService<ScreenLockService>();
        screenLock.LockRequested += () => DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                LockScreen.Visibility = Visibility.Visible;
                SessionOverlay.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update UI for lock screen");
            }
        });

        screenLock.UnlockRequested += () => DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                LockScreen.Visibility = Visibility.Collapsed;
                SessionOverlay.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update UI for unlock screen");
            }
        });
    }
}
