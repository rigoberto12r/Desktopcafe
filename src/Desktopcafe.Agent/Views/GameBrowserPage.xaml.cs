using Desktopcafe.Agent.Services;
using Desktopcafe.Core.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Desktopcafe.Agent.Views;

public sealed partial class GameBrowserPage : UserControl
{
    private List<GameDto> _games = new();

    public GameBrowserPage()
    {
        InitializeComponent();
    }

    public void UpdateGames(List<GameDto> games)
    {
        _games = games;
        DispatcherQueue.TryEnqueue(() => GamesGrid.ItemsSource = _games);
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        var search = sender.Text?.Trim();
        var filtered = string.IsNullOrEmpty(search)
            ? _games
            : _games.Where(g => g.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        GamesGrid.ItemsSource = filtered;
    }

    private async void Game_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is GameDto game)
        {
            var launcher = App.Services.GetRequiredService<GameLauncherService>();
            await launcher.LaunchGameAsync(game);
        }
    }
}
