using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace MicaForUWP.Helpers
{
    internal static class UIHelper
    {
        public static bool HasBackdropBrush = ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateBackdropBrush");
        public static bool HostBackdropBrush = ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush");
        public static bool HasWallpaperBackdropBrush = ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush");

        public static readonly UISettings UISettings = new UISettings();

        public static bool IsDarkTheme()
        {
            if (Window.Current?.Content is FrameworkElement frameworkElement)
            {
                return IsDarkTheme((Window.Current.Content as FrameworkElement).RequestedTheme);
            }
            else
            {
                return UISettings.GetColorValue(UIColorType.Background) == Windows.UI.Colors.Black;
            }
        }

        public static bool IsDarkTheme(ElementTheme theme) => theme == ElementTheme.Default ? Application.Current.RequestedTheme == ApplicationTheme.Dark : theme == ElementTheme.Dark;
    }
}
