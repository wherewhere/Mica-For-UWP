using MicaDemo.Pages;
using Windows.Foundation.Metadata;
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

        private void Mica_Click(object sender, RoutedEventArgs e)
        {
            //var testAppUri = new Uri("apkinstaller:"); // The protocol handled by the launched app
            //var options = new LauncherOptions();
            //options.TargetApplicationPackageFamilyName = "18184wherewhere.AndroidAppInstaller_4v4sx105x6y4r";

            //var inputData = new ValueSet();
            //inputData["FilePath"] = @"C:\Users\qq251\Downloads\Programs\weixin8020android2100_arm64_4.apk";

            //LaunchUriResult result = await Launcher.LaunchUriForResultsAsync(testAppUri, options, inputData);
            Frame.Navigate(typeof(MicaPage));
        }

        private void Blur_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BlurPage));
        }
    }
}
