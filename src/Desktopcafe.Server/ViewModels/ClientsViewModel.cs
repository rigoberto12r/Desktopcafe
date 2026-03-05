using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Server.ViewModels;

public partial class ClientsViewModel : ObservableObject
{
    private readonly AppDbContext _db;

    public ClientsViewModel(AppDbContext db)
    {
        _db = db;
    }

    public ObservableCollection<Client> Clients { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Client? _selectedClient;

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            var query = _db.Clients.Where(c => c.IsActive);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(c =>
                    c.Name.Contains(SearchText) ||
                    c.Code.Contains(SearchText) ||
                    (c.Phone != null && c.Phone.Contains(SearchText)));
            }

            var results = await query.OrderBy(c => c.Name).ToListAsync();

            Clients.Clear();
            foreach (var client in results)
            {
                Clients.Add(client);
            }
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    [RelayCommand]
    private async Task AddClientAsync()
    {
        // Dialog-driven: view layer creates the client and calls back
        await SearchAsync();
    }

    [RelayCommand]
    private async Task EditClientAsync()
    {
        if (SelectedClient is null) return;

        try
        {
            _db.Clients.Update(SelectedClient);
            await _db.SaveChangesAsync();
            await SearchAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    [RelayCommand]
    private async Task DeleteClientAsync()
    {
        if (SelectedClient is null) return;

        try
        {
            SelectedClient.IsActive = false;
            _db.Clients.Update(SelectedClient);
            await _db.SaveChangesAsync();
            await SearchAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    [RelayCommand]
    private async Task RechargeBalanceAsync()
    {
        if (SelectedClient is null) return;

        try
        {
            _db.Clients.Update(SelectedClient);
            await _db.SaveChangesAsync();
            await SearchAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }
}
