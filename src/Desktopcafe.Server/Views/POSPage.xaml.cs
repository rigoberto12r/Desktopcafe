using Desktopcafe.Core.Enums;
using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Serilog;

namespace Desktopcafe.Server.Views;

public sealed partial class POSPage : Page
{
    private List<Product> _allProducts = new();
    private readonly List<CartItem> _cart = new();

    public POSPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            using var db = new AppDbContext();
            _allProducts = await db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load products for POS");
        }
    }

    private void ApplyFilters()
    {
        var filtered = _allProducts.AsEnumerable();
        var search = SearchBox.Text?.Trim();
        if (!string.IsNullOrEmpty(search))
            filtered = filtered.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (CategoryFilter.SelectedIndex > 0)
        {
            var cat = (CategoryFilter.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (!string.IsNullOrEmpty(cat))
                filtered = filtered.Where(p => p.Category == cat);
        }

        ProductGrid.ItemsSource = filtered.Select(p => new
        {
            p.Id, p.Name,
            PriceText = $"${p.Price:F2}",
            StockText = $"Stock: {p.Stock}"
        }).ToList();
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) => ApplyFilters();
    private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

    private void ProductGrid_ItemClick(object sender, ItemClickEventArgs e)
    {
        dynamic item = e.ClickedItem;
        int productId = item.Id;
        var product = _allProducts.FirstOrDefault(p => p.Id == productId);
        if (product == null || product.Stock <= 0) return;

        var existing = _cart.FirstOrDefault(c => c.ProductId == productId);
        if (existing != null)
            existing.Quantity++;
        else
            _cart.Add(new CartItem { ProductId = productId, Name = product.Name, UnitPrice = product.Price, Quantity = 1 });

        RefreshCart();
    }

    private void RemoveCartItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int index && index < _cart.Count)
        {
            _cart.RemoveAt(index);
            RefreshCart();
        }
    }

    private void ClearCart_Click(object sender, RoutedEventArgs e) { _cart.Clear(); RefreshCart(); }

    private void RefreshCart()
    {
        CartList.ItemsSource = _cart.Select((c, i) => new
        {
            c.Name,
            QuantityText = $"x{c.Quantity}",
            SubtotalText = $"${c.Subtotal:F2}",
            Index = i
        }).ToList();
        CartTotalText.Text = $"${_cart.Sum(c => c.Subtotal):F2}";
    }

    private async void Checkout_Click(object sender, RoutedEventArgs e)
    {
        if (!_cart.Any()) return;

        var total = _cart.Sum(c => c.Subtotal);
        var payCombo = new ComboBox { Header = "Metodo de Pago", ItemsSource = new[] { "Efectivo", "Tarjeta", "Transferencia" }, SelectedIndex = 0, Width = 250 };
        var amountBox = new NumberBox { Header = "Monto recibido", Value = (double)total, Minimum = 0, Width = 250 };
        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(new TextBlock { Text = $"Total: ${total:F2}", FontSize = 24, FontWeight = Microsoft.UI.Text.FontWeights.Bold });
        panel.Children.Add(payCombo);
        panel.Children.Add(amountBox);

        var dialog = new ContentDialog
        {
            Title = "Cobrar",
            Content = panel,
            PrimaryButtonText = "Completar Venta",
            CloseButtonText = "Cancelar",
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            try
            {
                using var db = new AppDbContext();
                var sale = new Sale
                {
                    EmployeeId = App.CurrentEmployeeId,
                    Total = total,
                    PaymentMethod = (PaymentMethod)payCombo.SelectedIndex,
                    AmountPaid = (decimal)amountBox.Value,
                    Change = (decimal)amountBox.Value - total
                };

                foreach (var item in _cart)
                {
                    sale.Items.Add(new SaleItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Subtotal = item.Subtotal
                    });

                    var product = await db.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock -= item.Quantity;
                        db.Entry(product).State = EntityState.Modified;
                    }
                }

                await db.Sales.AddAsync(sale);
                await db.SaveChangesAsync();

                _cart.Clear();
                RefreshCart();
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to process sale with {ItemCount} items, total {Total}", _cart.Count, total);
                await new ContentDialog
                {
                    Title = "Error",
                    Content = "Error al procesar la venta.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = XamlRoot
                }.ShowAsync();
            }
        }
    }

    private class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;
    }
}
