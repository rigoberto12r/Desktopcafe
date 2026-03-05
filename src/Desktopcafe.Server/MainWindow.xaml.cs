using Desktopcafe.Server.Helpers;
using Desktopcafe.Server.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Desktopcafe.Server;

public sealed partial class MainWindow : Window
{
    private readonly Dictionary<string, Type> _pages = new()
    {
        { "Dashboard", typeof(DashboardPage) },
        { "ComputerMap", typeof(ComputerMapPage) },
        { "Timer", typeof(TimerPage) },
        { "POS", typeof(POSPage) },
        { "Clients", typeof(ClientsPage) },
        { "Printer", typeof(PrinterPage) },
        { "Inventory", typeof(InventoryPage) },
        { "Employees", typeof(EmployeesPage) },
        { "Reports", typeof(ReportsPage) },
        { "WiFi", typeof(WiFiPage) },
        { "Gaming", typeof(GamingPage) },
        { "Reservations", typeof(ReservationsPage) },
        { "Settings", typeof(SettingsPage) },
    };

    public MainWindow()
    {
        InitializeComponent();
        Title = "Desktopcafe";
        ExtendsContentIntoTitleBar = true;

        // Apply Mica backdrop
        WindowHelper.TrySetMicaBackdrop(this);
    }

    private void LoginOverlay_LoginSucceeded(object sender, EventArgs e)
    {
        LoginOverlay.Visibility = Visibility.Collapsed;
        NavView.Visibility = Visibility.Visible;

        // Navigate to Dashboard by default
        if (NavView.MenuItems.Count > 0)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(DashboardPage));
        }
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item &&
            item.Tag is string tag &&
            _pages.TryGetValue(tag, out var pageType))
        {
            ContentFrame.Navigate(pageType);
        }
    }
}
