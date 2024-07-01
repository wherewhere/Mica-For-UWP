﻿using MicaDemo.Common;
using MicaDemo.Helpers;
using MicaForUWP.Media;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MicaDemo.ViewModels
{
    public class BrushViewModel : INotifyPropertyChanged
    {
        public Thickness ScrollViewerMargin { get; } = UIHelper.ScrollViewerMargin;
        public Array BackgroundSources { get; } = Enum.GetValues(typeof(BackgroundSource));
        public bool IsAppWindowSupported { get; } = WindowHelper.IsAppWindowSupported;

        public CoreDispatcher Dispatcher { get; }

        private ICompactOverlay compactOverlay;
        public ICompactOverlay CompactOverlay
        {
            get => compactOverlay;
            set
            {
                if (compactOverlay != value)
                {
                    compactOverlay = value;
                    RaisePropertyChangedEvent(nameof(IsSupportCompactOverlay), nameof(IsCompactOverlay));
                }
            }
        }

        public bool IsSupportCompactOverlay => CompactOverlay?.IsSupportCompactOverlay == true;

        public bool IsCompactOverlay
        {
            get => IsSupportCompactOverlay && CompactOverlay?.IsCompactOverlay == true;
            set
            {
                if (IsSupportCompactOverlay)
                {
                    if (value)
                    {
                        CompactOverlay?.EnterCompactOverlay();
                    }
                    else
                    {
                        CompactOverlay?.ExitCompactOverlay();
                    }
                    RaisePropertyChangedEvent();
                }
            }
        }

        private ImageSource image;
        public ImageSource BackgroundImage
        {
            get => image;
            set
            {
                SetProperty(ref image, value);
            }
        }

        private bool isHideCard = false;
        public bool IsHideCard
        {
            get => isHideCard;
            set
            {
                SetProperty(ref isHideCard, value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        protected async void RaisePropertyChangedEvent(params string[] names)
        {
            if (names != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                names.ForEach(name => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
            }
        }

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        public BrushViewModel(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            BackgroundImage = new BitmapImage(new Uri("ms-appx:///Assets/Photos/BigFourSummerHeat.jpg"));
        }
    }

    public interface ICompactOverlay
    {
        bool IsSupportCompactOverlay { get; }
        bool IsCompactOverlay { get; }
        void EnterCompactOverlay();
        void ExitCompactOverlay();
    }

    public class CoreWindowCompactOverlay : ICompactOverlay
    {
        public bool IsSupportCompactOverlay =>
            ApiInformation.IsMethodPresent("Windows.UI.ViewManagement.ApplicationView", "IsViewModeSupported")
            && ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay);

        public bool IsCompactOverlay =>
            IsSupportCompactOverlay && ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.CompactOverlay;

        public void EnterCompactOverlay()
        {
            if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
            {
                _ = ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
            }
        }

        public void ExitCompactOverlay()
        {
            if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.Default))
            {
                _ = ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
            }
        }
    }

    public class AppWindowCompactOverlay : ICompactOverlay
    {
        private readonly AppWindow window;

        public AppWindowCompactOverlay(AppWindow window) => this.window = window;

        public bool IsSupportCompactOverlay =>
            ApiInformation.IsMethodPresent("Windows.UI.WindowManagement.AppWindowPresenter", "IsPresentationSupported")
            && window.Presenter.IsPresentationSupported(AppWindowPresentationKind.CompactOverlay);

        public bool IsCompactOverlay =>
            IsSupportCompactOverlay && window.Presenter.GetConfiguration().Kind == AppWindowPresentationKind.CompactOverlay;

        public void EnterCompactOverlay()
        {
            if (window.Presenter.IsPresentationSupported(AppWindowPresentationKind.CompactOverlay))
            {
                _ = window.Presenter.RequestPresentation(AppWindowPresentationKind.CompactOverlay);
            }
        }

        public void ExitCompactOverlay()
        {
            if (window.Presenter.IsPresentationSupported(AppWindowPresentationKind.CompactOverlay))
            {
                _ = window.Presenter.RequestPresentation(AppWindowPresentationKind.Default);
            }
        }
    }
}
