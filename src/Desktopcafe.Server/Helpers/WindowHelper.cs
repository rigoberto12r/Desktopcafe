using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;

namespace Desktopcafe.Server.Helpers;

public static class WindowHelper
{
    public static void TrySetMicaBackdrop(Window window)
    {
        if (MicaController.IsSupported())
        {
            var backdropController = new MicaController
            {
                Kind = MicaKind.Base
            };

            var configSource = new SystemBackdropConfiguration();
            configSource.IsInputActive = true;

            if (window.Content is FrameworkElement rootElement)
            {
                rootElement.ActualThemeChanged += (s, _) =>
                {
                    configSource.Theme = s.ActualTheme switch
                    {
                        ElementTheme.Dark => SystemBackdropTheme.Dark,
                        ElementTheme.Light => SystemBackdropTheme.Light,
                        _ => SystemBackdropTheme.Default
                    };
                };
            }

            backdropController.AddSystemBackdropTarget(window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            backdropController.SetSystemBackdropConfiguration(configSource);
        }
    }
}
