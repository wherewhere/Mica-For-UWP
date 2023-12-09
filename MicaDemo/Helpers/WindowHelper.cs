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

        public static void TrackWindow(this AppWindow window, Frame frame)
        {
            if (!ActiveAppWindows.ContainsKey(frame.Dispatcher))
            {
                ActiveAppWindows[frame.Dispatcher] = new Dictionary<UIElement, AppWindow>();
            }

            if (!ActiveAppWindows[frame.Dispatcher].ContainsKey(frame))
            {
                window.Closed += (sender, args) =>
                {
                    if (ActiveAppWindows.TryGetValue(frame.Dispatcher, out Dictionary<UIElement, AppWindow> windows))
                    {
                        windows?.Remove(frame);
                    }
                    frame.Content = null;
                    window = null;
                };
                ActiveAppWindows[frame.Dispatcher][frame] = window;
            }
        }

        public static UIElement GetXamlRootForWindow(this AppWindow window)
        {
            foreach (Dictionary<UIElement, AppWindow> windows in ActiveAppWindows.Values)
            {
                foreach (KeyValuePair<UIElement, AppWindow> element in windows)
                {
                    if (element.Value == window)
                    {
                        return element.Key;
                    }
                }
            }
            return null;
        }

        public static Dictionary<CoreDispatcher, Window> ActiveWindows { get; } = new Dictionary<CoreDispatcher, Window>();
        public static Dictionary<CoreDispatcher, Dictionary<UIElement, AppWindow>> ActiveAppWindows { get; } = IsAppWindowSupported ? new Dictionary<CoreDispatcher, Dictionary<UIElement, AppWindow>>() : null;
    }
}
