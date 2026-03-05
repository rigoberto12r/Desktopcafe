using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Desktopcafe.Server.Views;

public sealed partial class InventoryPage : Page
{
    private List<Product> _allProducts = new();

    public InventoryPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        using var db = new AppDbContext();
        _allProducts = await db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();

        var lowStock = _allProducts.Where(p => p.Stock <= p.MinStock).ToList();
        if (lowStock.Any())
        {
            LowStockAlert.IsOpen = true;
            LowStockAlert.Message = $"{lowStock.Count} producto(s) con stock bajo: {string.Join(", ", lowStock.Select(p => p.Name))}";
        }
        else
        {
            LowStockAlert.IsOpen = false;
        }

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var search = SearchBox.Text?.Trim();
        var filtered = _allProducts.AsEnumerable();

        if (!string.IsNullOrEmpty(search))
            filtered = filtered.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        ProductList.ItemsSource = filtered.Select(p => new
        {
            p.Id,
            p.Name,
            p.Category,
            PriceText = $"${p.Price:F2}",
            StockText = $"{p.Stock}",
            MinStockText = $"Min: {p.MinStock}",
            StockColor = p.Stock <= p.MinStock ? "Red" : "Green"
        }).ToList();
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) => ApplyFilter();

    private async void AddProduct_Click(object sender, RoutedEventArgs e)
    {
        var nameBox = new TextBox { Header = "Nombre" };
        var categoryCombo = new ComboBox
        {
            Header = "Categoria",
            ItemsSource = new[] { "Bebidas", "Snacks", "Papeleria", "Otros" },
            SelectedIndex = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        var priceBox = new NumberBox { Header = "Precio", Minimum = 0, Value = 0, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
        var costBox = new NumberBox { Header = "Costo", Minimum = 0, Value = 0, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
        var stockBox = new NumberBox { Header = "Stock Inicial", Minimum = 0, Value = 0, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
        var minStockBox = new NumberBox { Header = "Stock Minimo", Minimum = 0, Value = 5, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };

        var panel = new StackPanel { Spacing = 12, MinWidth = 350 };
        panel.Children.Add(nameBox);
        panel.Children.Add(categoryCombo);
        panel.Children.Add(priceBox);
        panel.Children.Add(costBox);
        panel.Children.Add(stockBox);
        panel.Children.Add(minStockBox);

        var dialog = new ContentDialog
        {
            Title = "Agregar Producto",
            Content = panel,
            PrimaryButtonText = "Agregar",
            CloseButtonText = "Cancelar",
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            var name = nameBox.Text?.Trim();
            if (string.IsNullOrEmpty(name)) return;

            var product = new Product
            {
                Name = name,
                Category = categoryCombo.SelectedItem?.ToString() ?? "Otros",
                Price = (decimal)priceBox.Value,
                Cost = (decimal)costBox.Value,
                Stock = (int)stockBox.Value,
                MinStock = (int)minStockBox.Value
            };

            using var db = new AppDbContext();
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();
            await LoadProductsAsync();
        }
    }

    private async void EditProduct_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int productId) return;

        using var db = new AppDbContext();
        var product = await db.Products.FindAsync(productId);
        if (product == null) return;

        var nameBox = new TextBox { Header = "Nombre", Text = product.Name };
        var categoryCombo = new ComboBox
        {
            Header = "Categoria",
            ItemsSource = new[] { "Bebidas", "Snacks", "Papeleria", "Otros" },
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        categoryCombo.SelectedItem = product.Category;
        if (categoryCombo.SelectedIndex < 0) categoryCombo.SelectedIndex = 3;

        var priceBox = new NumberBox { Header = "Precio", Minimum = 0, Value = (double)product.Price, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
        var costBox = new NumberBox { Header = "Costo", Minimum = 0, Value = (double)product.Cost, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
        var minStockBox = new NumberBox { Header = "Stock Minimo", Minimum = 0, Value = product.MinStock, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };

        var panel = new StackPanel { Spacing = 12, MinWidth = 350 };
        panel.Children.Add(nameBox);
        panel.Children.Add(categoryCombo);
        panel.Children.Add(priceBox);
        panel.Children.Add(costBox);
        panel.Children.Add(minStockBox);

        var dialog = new ContentDialog
        {
            Title = "Editar Producto",
            Content = panel,
            PrimaryButtonText = "Guardar",
            CloseButtonText = "Cancelar",
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            var name = nameBox.Text?.Trim();
            if (string.IsNullOrEmpty(name)) return;

            product.Name = name;
            product.Category = categoryCombo.SelectedItem?.ToString() ?? "Otros";
            product.Price = (decimal)priceBox.Value;
            product.Cost = (decimal)costBox.Value;
            product.MinStock = (int)minStockBox.Value;

            db.Entry(product).State = EntityState.Modified;
            await db.SaveChangesAsync();
            await LoadProductsAsync();
        }
    }

    private async void AddStock_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int productId) return;

        using var db = new AppDbContext();
        var product = await db.Products.FindAsync(productId);
        if (product == null) return;

        var quantityBox = new NumberBox
        {
            Header = $"Stock actual: {product.Stock}",
            PlaceholderText = "Cantidad a agregar",
            Minimum = 1,
            Value = 1,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
        };

        var dialog = new ContentDialog
        {
            Title = $"Agregar Stock - {product.Name}",
            Content = quantityBox,
            PrimaryButtonText = "Agregar",
            CloseButtonText = "Cancelar",
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            var qty = (int)quantityBox.Value;
            if (qty <= 0) return;

            product.Stock += qty;
            db.Entry(product).State = EntityState.Modified;
            await db.SaveChangesAsync();
            await LoadProductsAsync();
        }
    }
}
