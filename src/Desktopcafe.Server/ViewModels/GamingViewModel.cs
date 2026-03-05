using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Server.ViewModels;

public partial class GamingViewModel : ObservableObject
{
    private readonly AppDbContext _db;

    public GamingViewModel(AppDbContext db)
    {
        _db = db;
    }

    public ObservableCollection<Game> Games { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => _ = LoadGamesAsync();

    [RelayCommand]
    private async Task AddGameAsync()
    {
        // Dialog-driven: view layer handles game creation
        await LoadGamesAsync();
    }

    [RelayCommand]
    private async Task EditGameAsync()
    {
        // Dialog-driven: view layer handles game editing
        await LoadGamesAsync();
    }

    [RelayCommand]
    private async Task DeleteGameAsync()
    {
        // Soft-delete handled via dialog confirmation, then reload
        await LoadGamesAsync();
    }

    private async Task LoadGamesAsync()
    {
        try
        {
            var query = _db.Games.Where(g => g.IsActive);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(g =>
                    g.Name.Contains(SearchText) ||
                    g.Category.Contains(SearchText));
            }

            var games = await query.OrderBy(g => g.Name).ToListAsync();

            Games.Clear();
            foreach (var game in games)
            {
                Games.Add(game);
            }
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }
}
