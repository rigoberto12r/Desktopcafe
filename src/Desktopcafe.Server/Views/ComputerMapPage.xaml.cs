using Desktopcafe.Core.Enums;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Serilog;

namespace Desktopcafe.Server.Views;

public sealed partial class ComputerMapPage : Page
{
    public ComputerMapPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        await LoadComputersAsync();
    }

    private async Task LoadComputersAsync()
    {
        try
        {
            using var db = new AppDbContext();
            var computers = await db.Computers
                .Include(c => c.Sessions.Where(s => s.Status == SessionStatus.Active))
                .ThenInclude(s => s.Client)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var cards = computers.Select(c =>
            {
                var session = c.Sessions.FirstOrDefault();
                var remaining = session?.PlannedEnd != null ? session.PlannedEnd.Value - DateTime.Now : TimeSpan.Zero;
                var progress = session?.PlannedEnd != null && session.StartTime != default
                    ? (1.0 - remaining.TotalSeconds / (session.PlannedEnd.Value - session.StartTime).TotalSeconds) * 100
                    : 0;

                return new
                {
                    c.Id,
                    c.Name,
                    StatusText = c.Status switch
                    {
                        ComputerStatus.Available => "Disponible",
                        ComputerStatus.InUse => $"En uso",
                        ComputerStatus.Maintenance => "Mantenimiento",
                        _ => "Offline"
                    },
                    StatusColor = c.Status switch
                    {
                        ComputerStatus.Available => App.Current.Resources["StatusAvailableBrush"],
                        ComputerStatus.InUse => remaining.TotalMinutes <= 2
                            ? App.Current.Resources["StatusErrorBrush"]
                            : App.Current.Resources["StatusInUseBrush"],
                        ComputerStatus.Maintenance => App.Current.Resources["StatusMaintenanceBrush"],
                        _ => App.Current.Resources["StatusOfflineBrush"]
                    },
                    TimeRemainingText = session != null && remaining > TimeSpan.Zero
                        ? remaining.ToString(remaining.TotalHours >= 1 ? @"h\:mm\:ss" : @"mm\:ss")
                        : "",
                    ClientName = session?.Client?.Name ?? "",
                    HasActiveSession = session != null ? Visibility.Visible : Visibility.Collapsed,
                    Progress = Math.Clamp(progress, 0, 100)
                };
            }).ToList();

            ComputerGrid.ItemsSource = cards;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load computers");
        }
    }

    private async void ComputerGrid_ItemClick(object sender, ItemClickEventArgs e)
    {
        try
        {
            // Open session dialog for selected computer
            dynamic item = e.ClickedItem;
            int id = item.Id;

            var dialog = new ContentDialog
            {
                Title = $"Computadora {item.Name}",
                Content = $"Estado: {item.StatusText}",
                PrimaryButtonText = "Nueva Sesion",
                CloseButtonText = "Cerrar",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to show computer details dialog");
        }
    }

    private async void AddComputer_Click(object sender, RoutedEventArgs e)
    {
        var nameBox = new TextBox { Header = "Nombre", PlaceholderText = "Ej: PC-01" };
        var ipBox = new TextBox { Header = "IP Address", PlaceholderText = "192.168.1.100" };
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(nameBox);
        panel.Children.Add(ipBox);

        var dialog = new ContentDialog
        {
            Title = "Agregar Computadora",
            Content = panel,
            PrimaryButtonText = "Agregar",
            CloseButtonText = "Cancelar",
            XamlRoot = this.XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(nameBox.Text))
        {
            try
            {
                using var db = new AppDbContext();
                db.Computers.Add(new Core.Models.Computer
                {
                    Name = nameBox.Text.Trim(),
                    IpAddress = ipBox.Text?.Trim() ?? "",
                    Status = ComputerStatus.Offline
                });
                await db.SaveChangesAsync();
                await LoadComputersAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add computer {ComputerName}", nameBox.Text.Trim());
                await new ContentDialog
                {
                    Title = "Error",
                    Content = "Error al agregar la computadora.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = XamlRoot
                }.ShowAsync();
            }
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadComputersAsync();

    private void Card_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border)
            border.Scale = new System.Numerics.Vector3(1.03f, 1.03f, 1f);
    }

    private void Card_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border)
            border.Scale = new System.Numerics.Vector3(1f, 1f, 1f);
    }
}
