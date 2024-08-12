using Windows.UI.Xaml;

namespace MicaDemo.Helpers
{
    internal static class UIHelper
    {
        public static double TitleBarHeight { get; } = ThemeHelper.IsStatusBarSupported ? 4 : 32;
        public static Thickness ScrollViewerMargin => new Thickness(0, TitleBarHeight, 0, TitleBarHeight);
    }
}
