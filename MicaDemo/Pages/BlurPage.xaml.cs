using MicaDemo.Helpers;
using MicaForUWP.Media;
using System;
using Windows.UI;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MicaDemo.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BlurPage : Page
    {
        private readonly Thickness ScrollViewerMargin = UIHelper.ScrollViewerMargin;
        private readonly Array BackgroundSources = Enum.GetValues(typeof(BackgroundSource));
        private readonly Visibility NewWindowVisibility = WindowHelper.IsSupportedAppWindow ? Visibility.Visible : Visibility.Collapsed;

        public BlurPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ActualThemeChanged += OnActualThemeChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ActualThemeChanged -= OnActualThemeChanged;
        }

        private async void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.ResumeForegroundAsync();
            }

            TintColor.Color = Color.FromArgb(TintColor.Color.A, (byte)(255 - TintColor.Color.R), (byte)(255 - TintColor.Color.G), (byte)(255 - TintColor.Color.B));
        }

        private void TitleBar_BackRequested(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) { Frame.GoBack(); }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "NewWindow":
                    if (WindowHelper.IsSupportedAppWindow)
                    {
                        Tuple<AppWindow, Frame> window = await WindowHelper.CreateWindow();
                        window.Item1.TitleBar.ExtendsContentIntoTitleBar = true;
                        ThemeHelper.Initialize();
                        window.Item2.Navigate(typeof(MainPage));
                        await window.Item1.TryShowAsync();
                    }
                    break;
                case "ChangeTheme":
                    ThemeHelper.RootTheme = ThemeHelper.IsDarkTheme() ? ElementTheme.Light : ElementTheme.Dark;
                    break;
                default:
                    break;
            }
        }
    }
}
