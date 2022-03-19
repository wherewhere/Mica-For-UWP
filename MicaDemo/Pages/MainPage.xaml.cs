using MicaDemo.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            this.InitializeComponent();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar TitleBar = ApplicationView.GetForCurrentView().TitleBar;
            TitleBar.ButtonBackgroundColor = TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            if (!ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush"))
            {
                Mica.IsEnabled = false;
                BlurSymbol.Symbol = Symbol.Cancel;
                ToolTipService.SetToolTip(Mica, "不支持Mica");
            }
            if (!ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
            {
                Blur.IsEnabled = false;
                BlurSymbol.Symbol = Symbol.Cancel;
                ToolTipService.SetToolTip(Blur, "不支持高斯模糊");
            }
        }

        private async void Mica_Click(object sender, RoutedEventArgs e)
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
