using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktopcafe.Core.Interfaces;
using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Server.ViewModels;

public partial class POSViewModel : ObservableObject
{
    private readonly ICacheService _cacheService;
    private readonly AppDbContext _db;

    public POSViewModel(ICacheService cacheService, AppDbContext db)
    {
        _cacheService = cacheService;
        _db = db;
    }

    public ObservableCollection<Product> Products { get; } = new();

    public ObservableCollection<CartItemViewModel> CartItems { get; } = new();

    public ObservableCollection<string> Categories { get; } = new();

    [ObservableProperty]
    private string _selectedCategory = "All";

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private decimal _cartTotal;

    [RelayCommand]
    private async Task AddToCartAsync(int productId)
    {
        var product = Products.FirstOrDefault(p => p.Id == productId);
        if (product is null || product.Stock <= 0) return;

        var existing = CartItems.FirstOrDefault(c => c.ProductId == productId);
        if (existing is not null)
        {
            existing.Quantity++;
            existing.Subtotal = existing.Quantity * existing.UnitPrice;
        }
        else
        {
            CartItems.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                Name = product.Name,
                Quantity = 1,
                UnitPrice = product.Price,
                Subtotal = product.Price
            });
        }

        RecalculateTotal();
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void RemoveFromCart(int index)
    {
        if (index >= 0 && index < CartItems.Count)
        {
            CartItems.RemoveAt(index);
            RecalculateTotal();
        }
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (CartItems.Count == 0) return;

        try
        {
            var sale = new Sale
            {
                EmployeeId = App.CurrentEmployeeId,
                Total = CartTotal,
                CreatedAt = DateTime.Now
            };

            _db.Sales.Add(sale);
            await _db.SaveChangesAsync();

            foreach (var item in CartItems)
            {
                var saleItem = new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal
                };
                _db.SaleItems.Add(saleItem);

                var product = await _db.Products.FindAsync(item.ProductId);
                if (product is not null)
                {
                    product.Stock -= item.Quantity;
                    _db.Products.Update(product);
                }
            }

            await _db.SaveChangesAsync();

            CartItems.Clear();
            CartTotal = 0;
            _cacheService.InvalidateDashboard();
            _cacheService.InvalidateProducts();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    [RelayCommand]
    private void ClearCart()
    {
        CartItems.Clear();
        CartTotal = 0;
    }

    private void RecalculateTotal()
    {
        CartTotal = CartItems.Sum(c => c.Subtotal);
    }

    partial void OnSelectedCategoryChanged(string value) => _ = FilterProductsAsync();

    partial void OnSearchTextChanged(string value) => _ = FilterProductsAsync();

    private async Task FilterProductsAsync()
    {
        try
        {
            var allProducts = await _cacheService.GetActiveProductsAsync();

            var filtered = allProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SelectedCategory) && SelectedCategory != "All")
            {
                filtered = filtered.Where(p => p.Category == SelectedCategory);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(p =>
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            Products.Clear();
            foreach (var product in filtered)
            {
                Products.Add(product);
            }
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    public partial class CartItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _productId;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private int _quantity;

        [ObservableProperty]
        private decimal _unitPrice;

        [ObservableProperty]
        private decimal _subtotal;
    }
}
