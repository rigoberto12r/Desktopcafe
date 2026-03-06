using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Serilog;

namespace Desktopcafe.Server.Views;

public sealed partial class GamingPage : Page
{
    public GamingPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadGamesAsync();
    }

    private async Task LoadGamesAsync()
    {
        try
        {
            using var db = new AppDbContext();
            var games = await db.Games.Where(g => g.IsActive).OrderBy(g => g.Name).ToListAsync();

            GamesGrid.ItemsSource = games.Select(g => new
            {
                g.Id,
                g.Name,
                g.Category,
                PlaysText = $"{g.TimesPlayed} partidas"
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load games");
        }
    }

    private async void AddGame_Click(object sender, RoutedEventArgs e)
    {
        var nameBox = new TextBox { Header = "Nombre" };
        var categoryCombo = new ComboBox
        {
            Header = "Categoria",
            ItemsSource = new[] { "FPS", "MOBA", "RPG", "Estrategia", "Deportes", "Carreras", "Aventura", "Otros" },
            SelectedIndex = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        var exePathBox = new TextBox { Header = "Ruta del Ejecutable", PlaceholderText = @"C:\Games\game.exe" };

        var panel = new StackPanel { Spacing = 12, MinWidth = 400 };
        panel.Children.Add(nameBox);
        panel.Children.Add(categoryCombo);
        panel.Children.Add(exePathBox);

        var dialog = new ContentDialog
        {
            Title = "Agregar Juego",
            Content = panel,
            PrimaryButtonText = "Agregar",
            CloseButtonText = "Cancelar",
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            var name = nameBox.Text?.Trim();
            if (string.IsNullOrEmpty(name)) return;

            try
            {
                var game = new Game
                {
                    Name = name,
                    Category = categoryCombo.SelectedItem?.ToString() ?? "Otros",
                    ExePath = exePathBox.Text?.Trim() ?? ""
                };

                using var db = new AppDbContext();
                await db.Games.AddAsync(game);
                await db.SaveChangesAsync();
                await LoadGamesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add game {Name}", name);
                await new ContentDialog
                {
                    Title = "Error",
                    Content = "No se pudo agregar el juego.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = XamlRoot
                }.ShowAsync();
            }
        }
    }
}
