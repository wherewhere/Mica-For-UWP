using MicaDemo.Common;
using MicaDemo.Helpers;
using MicaDemo.ViewModels;
using MicaForUWP.Media;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MicaDemo.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BlurPage : Page
    {
        private readonly BrushViewModel Provider;

        private bool isDark;
        private long token;

        private bool IsHideCard
        {
            get => Provider.IsHideCard;
            set
            {
                if (Provider.IsHideCard != value)
                {
                    Provider.IsHideCard = value;
                    VisualStateManager.GoToState(this, value ? "HideCard" : "DisplayCard", true);
                }
            }
        }

        public BlurPage()
        {
            InitializeComponent();
            Provider = new BrushViewModel(Dispatcher) { SelectSource = MicaForUWP.Media.BackgroundSource.Backdrop };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ThemeHelper.UISettingChanged += OnUISettingChanged;
            token = BackdropBrush.RegisterPropertyChangedCallback(BackdropBlurBrush.TintColorProperty, OnTintColorPropertyChanged);
            OnTintColorPropertyChanged(BackdropBrush, BackdropBlurBrush.TintColorProperty);
            VisualStateManager.GoToState(this, "DisplayCard", false);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ThemeHelper.UISettingChanged -= OnUISettingChanged;
            BackdropBrush.UnregisterPropertyChangedCallback(BackdropBlurBrush.TintColorProperty, token);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _ = ThemeHelper.GetRootThemeAsync().ContinueWith(x => x.Result.IsDarkTheme()).ContinueWith(x => isDark = x.Result);
            Provider.CompactOverlay = this.IsAppWindow() ? (ICompactOverlay)new AppWindowCompactOverlay(this.GetWindowForElement()) : new CoreWindowCompactOverlay();
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
                case "ChangeImage":
                    _ = Provider.PickImageAsync();
                    return;
                case "RemoveImage":
                    Provider.BackgroundImage = null;
                    break;
                case "HideSetting":
                    IsHideCard = true;
                    break;
                case "ShowSetting":
                    IsHideCard = false;
                    break;
                default:
                    break;
            }
        }

        private void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            IsHideCard = !IsHideCard;
            if (e != null) { e.Handled = true; }
        }
    }
}
