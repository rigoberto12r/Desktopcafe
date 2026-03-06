using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Serilog;

namespace Desktopcafe.Server.Views;

public sealed partial class LoginPage : UserControl
{
    public event EventHandler? LoginSucceeded;

    public LoginPage()
    {
        InitializeComponent();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        await AttemptLoginAsync();
    }

    private async void Input_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            await AttemptLoginAsync();
    }

    private async Task AttemptLoginAsync()
    {
        var username = UsernameBox.Text?.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Ingrese usuario y contrasena");
            return;
        }

        LoginButton.IsEnabled = false;
        LoadingRing.IsActive = true;

        try
        {
            using var db = new AppDbContext();
            var employee = await db.Employees
                .FirstOrDefaultAsync(e => e.Username == username && e.IsActive);

            if (employee == null || !BCrypt.Net.BCrypt.Verify(password, employee.PasswordHash))
            {
                ShowError("Usuario o contrasena incorrectos");
                return;
            }

            App.CurrentEmployeeId = employee.Id;
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Login failed for user {Username}", username);
            ShowError($"Error de conexion: {ex.Message}");
        }
        finally
        {
            LoginButton.IsEnabled = true;
            LoadingRing.IsActive = false;
        }
    }

    private void ShowError(string message)
    {
        ErrorInfoBar.Message = message;
        ErrorInfoBar.IsOpen = true;
    }
}
