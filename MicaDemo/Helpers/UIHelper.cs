using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;

namespace MicaDemo.Helpers
{
    internal static class UIHelper
    {
        public static bool HasStatusBar => ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        public static double TitleBarHeight => 32;
        public static double PageTitleHeight => HasStatusBar ? 48 : 48 + TitleBarHeight;
        public static Thickness StackPanelMargin => new Thickness(0, PageTitleHeight, 0, 0);
        public static Thickness ScrollViewerMargin => new Thickness(0, PageTitleHeight, 0, 0);
        public static Thickness ScrollViewerPadding => new Thickness(0, -PageTitleHeight, 0, 0);
    }
}
