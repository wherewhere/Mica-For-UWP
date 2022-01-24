using MicaDemo.Helpers;
using MicaForUWP.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MicaDemo.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MicaPage : Page
    {
        private Thickness ScrollViewerMargin => UIHelper.ScrollViewerMargin;
        private List<BackgroundSource> BackgroundSources = new List<BackgroundSource>()
        {
            MicaForUWP.Media.BackgroundSource.Backdrop,
            MicaForUWP.Media.BackgroundSource.HostBackdrop,
            MicaForUWP.Media.BackgroundSource.MicaBackdrop,
        };

        public MicaPage()
        {
            this.InitializeComponent();
        }

        private void TitleBar_BackRequested(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) { Frame.GoBack(); }
        }
    }
}
