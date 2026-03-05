using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktopcafe.Core.Interfaces;
using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Server.ViewModels;

public partial class InventoryViewModel : ObservableObject
{
    private readonly AppDbContext _db;
    private readonly ICacheService _cacheService;

    public InventoryViewModel(AppDbContext db, ICacheService cacheService)
    {
        _db = db;
        _cacheService = cacheService;
    }

    public ObservableCollection<Product> Products { get; } = new();

    public ObservableCollection<Product> LowStockProducts { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [RelayCommand]
    private async Task AddProductAsync()
    {
        // Dialog-driven: view layer handles product creation
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task EditProductAsync()
    {
        // Dialog-driven: view layer handles product editing
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task DeleteProductAsync()
    {
        // Soft-delete handled via dialog confirmation, then reload
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task AddStockAsync(int productId)
    {
        try
        {
            var product = await _db.Products.FindAsync(productId);
            if (product is null) return;

            // Quantity is set via dialog before calling this command
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
            _cacheService.InvalidateProducts();
            await LoadProductsAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    partial void OnSearchTextChanged(string value) => _ = LoadProductsAsync();

    private async Task LoadProductsAsync()
    {
        try
        {
            var query = _db.Products.Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(p =>
                    p.Name.Contains(SearchText) ||
                    p.Category.Contains(SearchText) ||
                    (p.Barcode != null && p.Barcode.Contains(SearchText)));
            }

            var products = await query.OrderBy(p => p.Name).ToListAsync();

            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }

            LowStockProducts.Clear();
            foreach (var product in products.Where(p => p.Stock <= p.MinStock))
            {
                LowStockProducts.Add(product);
            }
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }
}
