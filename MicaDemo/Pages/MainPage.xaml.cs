using MicaDemo.Pages;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MicaDemo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            { HardwareButtons.BackPressed += System_BackPressed; }
            if (!ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush"))
            {
                MicaSymbol.Symbol = Symbol.Cancel;
                ToolTipService.SetToolTip(Mica, "Not Support Mica");
            }
            if (!ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
            {
                BlurSymbol.Symbol = Symbol.Cancel;
                Mica.IsEnabled = Blur.IsEnabled = false;
                ToolTipService.SetToolTip(Blur, "Not Support Blur");
            }
        }

        private void System_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = TryGoBack();
            }
        }

        private void System_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = TryGoBack();
            }
        }

        private bool TryGoBack(bool goBack = true)
        {
            if (!Dispatcher.HasThreadAccess || !Frame.CanGoBack)
            { return false; }

            if (goBack) { Frame.GoBack(); }
            return true;
        }

        private void Mica_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MicaPage));
        }

        private void Blur_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BlurPage));
        }
    }
}
