using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;

namespace MicaDemo.Helpers
{
    internal static class UIHelper
    {
        public static bool HasTitleBar => !CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
        public static bool HasStatusBar { get; } = ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        public static double TitleBarHeight => HasStatusBar ? 4 : 32;
        public static Thickness ScrollViewerMargin => new Thickness(0, TitleBarHeight, 0, TitleBarHeight);
    }
}
