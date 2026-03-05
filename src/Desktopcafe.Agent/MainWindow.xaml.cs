using Desktopcafe.Agent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

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
            LockScreen.Visibility = Visibility.Visible;
            SessionOverlay.Visibility = Visibility.Collapsed;
        });

        screenLock.UnlockRequested += () => DispatcherQueue.TryEnqueue(() =>
        {
            LockScreen.Visibility = Visibility.Collapsed;
            SessionOverlay.Visibility = Visibility.Visible;
        });
    }
}
