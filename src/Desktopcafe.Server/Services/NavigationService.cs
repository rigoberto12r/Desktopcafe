using Serilog;

namespace Desktopcafe.Server.Services;

public class NavigationService
{
    private Type? _currentPage;

    public Type? CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (_currentPage == value) return;
            _currentPage = value;
            Navigated?.Invoke(this, value!);
        }
    }

    public event EventHandler<Type>? Navigated;

    public void NavigateTo<T>() where T : class
    {
        try
        {
            CurrentPage = typeof(T);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to navigate to {PageType}", typeof(T).Name);
            throw;
        }
    }

    public void NavigateTo(Type pageType)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(pageType);
            CurrentPage = pageType;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to navigate to {PageType}", pageType?.Name ?? "null");
            throw;
        }
    }
}
