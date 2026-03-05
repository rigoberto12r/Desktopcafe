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
        CurrentPage = typeof(T);
    }

    public void NavigateTo(Type pageType)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        CurrentPage = pageType;
    }
}
