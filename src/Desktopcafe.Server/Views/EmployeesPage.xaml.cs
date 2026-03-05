using Desktopcafe.Core.Enums;
using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Desktopcafe.Server.Views;

public sealed partial class EmployeesPage : Page
{
    private bool _isLoading;

    public EmployeesPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadEmployeesAsync();
    }

    private async Task LoadEmployeesAsync()
    {
        _isLoading = true;
        using var db = new AppDbContext();
        var employees = await db.Employees.OrderBy(e => e.Name).ToListAsync();

        EmployeeList.ItemsSource = employees.Select(emp => new
        {
            emp.Id,
            emp.Name,
            emp.Username,
            RoleText = emp.Role == UserRole.Admin ? "Administrador" : "Cajero",
            emp.IsActive,
            StatusText = emp.IsActive ? "Activo" : "Inactivo",
            StatusColor = emp.IsActive ? "Green" : "Gray"
        }).ToList();
        _isLoading = false;
    }

    private async void AddEmployee_Click(object sender, RoutedEventArgs e)
    {
        var nameBox = new TextBox { Header = "Nombre" };
        var usernameBox = new TextBox { Header = "Usuario" };
        var passwordBox = new PasswordBox { Header = "Contrasena" };
        var roleCombo = new ComboBox
        {
            Header = "Rol",
            ItemsSource = new[] { "Cajero", "Administrador" },
            SelectedIndex = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var panel = new StackPanel { Spacing = 12, MinWidth = 350 };
        panel.Children.Add(nameBox);
        panel.Children.Add(usernameBox);
        panel.Children.Add(passwordBox);
        panel.Children.Add(roleCombo);

        var dialog = new ContentDialog
        {
            Title = "Agregar Empleado",
            Content = panel,
            PrimaryButtonText = "Agregar",
            CloseButtonText = "Cancelar",
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            var name = nameBox.Text?.Trim();
            var username = usernameBox.Text?.Trim();
            var password = passwordBox.Password;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return;

            var employee = new Employee
            {
                Name = name,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = roleCombo.SelectedIndex == 1 ? UserRole.Admin : UserRole.Cashier
            };

            using var db = new AppDbContext();
            await db.Employees.AddAsync(employee);
            await db.SaveChangesAsync();
            await LoadEmployeesAsync();
        }
    }

    private async void ToggleActive_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        if (sender is not ToggleSwitch toggle || toggle.Tag is not int employeeId) return;

        using var db = new AppDbContext();
        var employee = await db.Employees.FindAsync(employeeId);
        if (employee == null) return;

        employee.IsActive = toggle.IsOn;
        db.Entry(employee).State = EntityState.Modified;
        await db.SaveChangesAsync();
    }
}
