using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Serilog;

namespace Desktopcafe.Server.Views;

public sealed partial class ClientsPage : Page
{
    private List<Client> _allClients = new();

    public ClientsPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadClientsAsync();
    }

    private async Task LoadClientsAsync()
    {
        try
        {
            using var db = new AppDbContext();
            _allClients = await db.Clients.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load clients");
        }
    }

    private void ApplyFilter()
    {
        var search = SearchBox.Text?.Trim();
        var filtered = _allClients.AsEnumerable();

        if (!string.IsNullOrEmpty(search))
        {
            filtered = filtered.Where(c =>
                c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (c.Phone != null && c.Phone.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                c.Code.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        ClientList.ItemsSource = filtered.Select(c => new
        {
            c.Id,
            c.Name,
            Phone = c.Phone ?? "-",
            Email = c.Email ?? "-",
            c.Code,
            BalanceText = $"${c.Balance:F2}",
            PointsText = $"{c.Points} pts"
        }).ToList();
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) => ApplyFilter();

    private async void AddClient_Click(object sender, RoutedEventArgs e)
    {
        var nameBox = new TextBox { Header = "Nombre", PlaceholderText = "Nombre completo" };
        var phoneBox = new TextBox { Header = "Telefono", PlaceholderText = "Numero de telefono" };
        var emailBox = new TextBox { Header = "Email", PlaceholderText = "correo@ejemplo.com" };

        var panel = new StackPanel { Spacing = 12, MinWidth = 350 };
        panel.Children.Add(nameBox);
        panel.Children.Add(phoneBox);
        panel.Children.Add(emailBox);

        var dialog = new ContentDialog
        {
            Title = "Agregar Cliente",
            Content = panel,
            PrimaryButtonText = "Agregar",
            CloseButtonText = "Cancelar",
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            var name = nameBox.Text?.Trim();
            if (string.IsNullOrEmpty(name)) return;

            var client = new Client
            {
                Name = name,
                Phone = string.IsNullOrWhiteSpace(phoneBox.Text) ? null : phoneBox.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(emailBox.Text) ? null : emailBox.Text.Trim(),
                Code = GenerateCode()
            };

            try
            {
                using var db = new AppDbContext();
                await db.Clients.AddAsync(client);
                await db.SaveChangesAsync();
                await LoadClientsAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add client {ClientName}", name);
                await new ContentDialog
                {
                    Title = "Error",
                    Content = "Error al agregar el cliente.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = XamlRoot
                }.ShowAsync();
            }
        }
    }

    private async void EditClient_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int clientId) return;

        try
        {
            using var db = new AppDbContext();
            var client = await db.Clients.FindAsync(clientId);
            if (client == null) return;

            var nameBox = new TextBox { Header = "Nombre", Text = client.Name };
            var phoneBox = new TextBox { Header = "Telefono", Text = client.Phone ?? "" };
            var emailBox = new TextBox { Header = "Email", Text = client.Email ?? "" };

            var panel = new StackPanel { Spacing = 12, MinWidth = 350 };
            panel.Children.Add(nameBox);
            panel.Children.Add(phoneBox);
            panel.Children.Add(emailBox);

            var dialog = new ContentDialog
            {
                Title = "Editar Cliente",
                Content = panel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                var name = nameBox.Text?.Trim();
                if (string.IsNullOrEmpty(name)) return;

                client.Name = name;
                client.Phone = string.IsNullOrWhiteSpace(phoneBox.Text) ? null : phoneBox.Text.Trim();
                client.Email = string.IsNullOrWhiteSpace(emailBox.Text) ? null : emailBox.Text.Trim();

                db.Entry(client).State = EntityState.Modified;
                await db.SaveChangesAsync();
                await LoadClientsAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to edit client {ClientId}", clientId);
            await new ContentDialog
            {
                Title = "Error",
                Content = "Error al editar el cliente.",
                CloseButtonText = "Aceptar",
                XamlRoot = XamlRoot
            }.ShowAsync();
        }
    }

    private async void RechargeBalance_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int clientId) return;

        try
        {
            using var db = new AppDbContext();
            var client = await db.Clients.FindAsync(clientId);
            if (client == null) return;

            var amountBox = new NumberBox
            {
                Header = $"Saldo actual: ${client.Balance:F2}",
                PlaceholderText = "Monto a recargar",
                Minimum = 0.01,
                Value = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };

            var dialog = new ContentDialog
            {
                Title = $"Recargar Saldo - {client.Name}",
                Content = amountBox,
                PrimaryButtonText = "Recargar",
                CloseButtonText = "Cancelar",
                XamlRoot = XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                var amount = (decimal)amountBox.Value;
                if (amount <= 0) return;

                client.Balance += amount;
                db.Entry(client).State = EntityState.Modified;
                await db.SaveChangesAsync();
                await LoadClientsAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to recharge balance for client {ClientId}", clientId);
            await new ContentDialog
            {
                Title = "Error",
                Content = "Error al recargar el saldo.",
                CloseButtonText = "Aceptar",
                XamlRoot = XamlRoot
            }.ShowAsync();
        }
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}
