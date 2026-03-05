using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Server.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AppDbContext _db;

    public LoginViewModel(AppDbContext db)
    {
        _db = db;
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public event EventHandler? LoginSucceeded;

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required.";
            return;
        }

        IsLoading = true;

        try
        {
            var employee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Username == Username && e.IsActive);

            if (employee is null)
            {
                ErrorMessage = "Invalid username or password.";
                return;
            }

            if (!BCrypt.Net.BCrypt.Verify(Password, employee.PasswordHash))
            {
                ErrorMessage = "Invalid username or password.";
                return;
            }

            App.CurrentEmployeeId = employee.Id;
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
