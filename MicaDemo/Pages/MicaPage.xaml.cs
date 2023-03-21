using MicaDemo.Helpers;
using MicaForUWP.Media;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MicaDemo.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MicaPage : Page
    {
        private Thickness ScrollViewerMargin => UIHelper.ScrollViewerMargin;
        private readonly List<BackgroundSource> BackgroundSources = new List<BackgroundSource>()
        {
            MicaForUWP.Media.BackgroundSource.Backdrop,
            MicaForUWP.Media.BackgroundSource.HostBackdrop,
            MicaForUWP.Media.BackgroundSource.WallpaperBackdrop,
        };

        public MicaPage() => InitializeComponent();

        private void TitleBar_BackRequested(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) { Frame.GoBack(); }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ThemeHelper.RootTheme = ThemeHelper.IsDarkTheme() ? ElementTheme.Light : ElementTheme.Dark;
            TintColor.Color = Color.FromArgb(TintColor.Color.A, (byte)(255 - TintColor.Color.R), (byte)(255 - TintColor.Color.G), (byte)(255 - TintColor.Color.B));
        }
    }
}
