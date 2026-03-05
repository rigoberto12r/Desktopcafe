using Microsoft.UI.Xaml;

namespace Desktopcafe.Server.Services;

public class ThemeService
{
    private ElementTheme _currentTheme = ElementTheme.Default;

    public ElementTheme CurrentTheme
    {
        get => _currentTheme;
        private set
        {
            if (_currentTheme == value) return;
            _currentTheme = value;
            ThemeChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<ElementTheme>? ThemeChanged;

    public void SetTheme(ElementTheme theme)
    {
        CurrentTheme = theme;
        ApplyTheme(theme);
    }

    public void ToggleTheme()
    {
        var next = CurrentTheme switch
        {
            ElementTheme.Light => ElementTheme.Dark,
            ElementTheme.Dark => ElementTheme.Default,
            _ => ElementTheme.Light
        };
        SetTheme(next);
    }

    private static void ApplyTheme(ElementTheme theme)
    {
        if (App.MainWindow?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = theme;
        }
    }
}
