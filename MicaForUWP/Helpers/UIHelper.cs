using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace MicaForUWP.Helpers
{
    internal static class UIHelper
    {
        internal static readonly UISettings UISettings = new UISettings();

        internal static bool IsDarkTheme()
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

        internal static bool IsDarkTheme(ElementTheme theme) => theme == ElementTheme.Default ? Application.Current.RequestedTheme == ApplicationTheme.Dark : theme == ElementTheme.Dark;
    }
}
