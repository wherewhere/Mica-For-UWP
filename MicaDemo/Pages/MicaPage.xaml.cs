using MicaDemo.Common;
using MicaDemo.Helpers;
using MicaForUWP.Media;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MicaDemo.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MicaPage : Page
    {
        private readonly Thickness ScrollViewerMargin = UIHelper.ScrollViewerMargin;
        private readonly Array BackgroundSources = Enum.GetValues(typeof(BackgroundSource));
        private readonly bool IsAppWindowSupported = WindowHelper.IsAppWindowSupported;

        private bool isDark;
        private long token;

        public MicaPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ThemeHelper.UISettingChanged += OnUISettingChanged;
            token = BackdropMicaBrush.RegisterPropertyChangedCallback(BackdropMicaBrush.TintColorProperty, OnTintColorPropertyChanged);
            OnTintColorPropertyChanged(BackdropMicaBrush, BackdropMicaBrush.TintColorProperty);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ThemeHelper.UISettingChanged -= OnUISettingChanged;
            BackdropMicaBrush.UnregisterPropertyChangedCallback(BackdropMicaBrush.TintColorProperty, token);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _ = ThemeHelper.GetRootThemeAsync().ContinueWith(x => x.Result.IsDarkTheme()).ContinueWith(x => isDark = x.Result);
        }

        private async void OnUISettingChanged(bool theme)
        {
            if (isDark != theme)
            {
                await Dispatcher.ResumeForegroundAsync();
                TintColor.Color = ReversalColor(TintColor.Color);
                isDark = theme;
            }

            Color ReversalColor(in Color color)
            {
                return Color.FromArgb(color.A, (byte)(255 - color.R), (byte)(255 - color.G), (byte)(255 - color.B));
            }
        }

        private void OnTintColorPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            Color color = (Color)sender.GetValue(dp);
            bool isLight = color.IsColorLight();
            RequestedTheme = isLight ? ElementTheme.Light : ElementTheme.Dark;
        }

        private void TitleBar_BackRequested(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) { Frame.GoBack(); }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag?.ToString())
            {
                case "NewWindow":
                    _ = await WindowHelper.CreateWindowAsync(window =>
                    {
                        CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                        Frame _frame = new Frame();
                        window.Content = _frame;
                        ThemeHelper.Initialize(window);
                        _ = _frame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
                    });
                    break;
                case "NewAppWindow" when WindowHelper.IsAppWindowSupported:
                    Tuple<AppWindow, Frame> appWindow = await WindowHelper.CreateWindowAsync();
                    appWindow.Item1.TitleBar.ExtendsContentIntoTitleBar = true;
                    ThemeHelper.Initialize(appWindow.Item1);
                    appWindow.Item2.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
                    await appWindow.Item1.TryShowAsync();
                    break;
                case "ChangeTheme":
                    _ = ThemeHelper.IsDarkThemeAsync().ContinueWith(x => ThemeHelper.SetRootThemeAsync(x.Result ? ElementTheme.Light : ElementTheme.Dark));
                    break;
                default:
                    break;
            }
        }
    }
}
