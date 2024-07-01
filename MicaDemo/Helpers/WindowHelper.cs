using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace MicaDemo.Helpers
{
    /// <summary>
    /// Helpers class to allow the app to find the Window that contains an
    /// arbitrary <see cref="UIElement"/> (<see cref="GetWindowForElement(UIElement)"/>).
    /// To do this, we keep track of all active Windows. The app code must call
    /// <see cref="CreateWindowAsync(Action{Window})"/> rather than "new <see cref="Window"/>()"
    /// so we can keep track of all the relevant windows.
    /// </summary>
    public static class WindowHelper
    {
        public static bool IsAppWindowSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.WindowManagement.AppWindow");
        public static bool IsXamlRootSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "XamlRoot");

        public static async Task<bool> CreateWindowAsync(Action<Window> launched)
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Window newWindow = Window.Current;
                launched(newWindow);
                TrackWindow(newWindow);
                Window.Current.Activate();
                newViewId = ApplicationView.GetForCurrentView().Id;
            });
            return await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }

        public static async Task<Tuple<AppWindow, Frame>> CreateWindowAsync()
        {
            Frame newFrame = new Frame();
            AppWindow newWindow = await AppWindow.TryCreateAsync();
            ElementCompositionPreview.SetAppWindowContent(newWindow, newFrame);
            TrackWindow(newWindow, newFrame);
            return new Tuple<AppWindow, Frame>(newWindow, newFrame);
        }

        public static void TrackWindow(this Window window)
        {
            if (!ActiveWindows.ContainsKey(window.Dispatcher))
            {
                window.Closed += (sender, args) =>
                {
                    ActiveWindows.Remove(window.Dispatcher);
                    window = null;
                };
                ActiveWindows[window.Dispatcher] = window;
            }
        }

        private static void TrackWindow(this AppWindow window, Frame frame)
        {
            if (!ActiveAppWindows.TryGetValue(frame.Dispatcher, out Dictionary<XamlRoot, AppWindow> windows))
            {
                ActiveAppWindows[frame.Dispatcher] = windows = new Dictionary<XamlRoot, AppWindow>();
            }

            if (!windows.ContainsKey(frame.XamlRoot))
            {
                window.Closed += (sender, args) =>
                {
                    windows.Remove(frame.XamlRoot);
                    if (windows.Count <= 0)
                    { ActiveAppWindows.Remove(frame.Dispatcher); }
                    frame.Content = null;
                    window = null;
                };
                windows[frame.XamlRoot] = window;
            }
        }

        public static bool IsAppWindow(this UIElement element) =>
            IsAppWindowSupported
            && element?.XamlRoot != null
            && ActiveAppWindows.TryGetValue(element.Dispatcher, out Dictionary<XamlRoot, AppWindow> windows)
            && windows.ContainsKey(element.XamlRoot);

        public static AppWindow GetWindowForElement(this UIElement element) =>
            IsAppWindowSupported
            && element?.XamlRoot != null
            && ActiveAppWindows.TryGetValue(element.Dispatcher, out Dictionary<XamlRoot, AppWindow> windows)
            && windows.TryGetValue(element.XamlRoot, out AppWindow window)
                ? window : null;

        public static UIElement GetXamlRootForWindow(this AppWindow window) => ElementCompositionPreview.GetAppWindowContent(window);

        public static Dictionary<CoreDispatcher, Window> ActiveWindows { get; } = new Dictionary<CoreDispatcher, Window>();
        public static Dictionary<CoreDispatcher, Dictionary<XamlRoot, AppWindow>> ActiveAppWindows { get; } = IsAppWindowSupported ? new Dictionary<CoreDispatcher, Dictionary<XamlRoot, AppWindow>>() : null;
    }
}
