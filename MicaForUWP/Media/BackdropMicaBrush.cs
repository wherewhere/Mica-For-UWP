using MicaForUWP.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using System;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.System.Power;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace MicaForUWP.Media
{
    /// <summary>
    /// The <see cref="BackdropMicaBrush"/> is a <see cref="Brush"/> that blurs whatever is behind it in the application.
    /// </summary>
    [ContractVersion(typeof(UniversalApiContract), 262144)]
    public class BackdropMicaBrush : XamlCompositionBrushBase
    {
        private bool _isForce = true;

        private CompositionEffectBrush Brush;
        private ScalarKeyFrameAnimation TintOpacityFillAnimation;
        private ScalarKeyFrameAnimation HostOpacityZeroAnimation;
        private ColorKeyFrameAnimation TintToFallBackAnimation;

        /// <summary>
        /// Gets or sets the tint for the effect
        /// </summary>
        public Color TintColor
        {
            get => (Color)GetValue(TintColorProperty);
            set => SetValue(TintColorProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="TintColor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TintColorProperty = DependencyProperty.Register(
            nameof(TintColor),
            typeof(Color),
            typeof(BackdropMicaBrush),
            new PropertyMetadata(UIHelper.IsDarkTheme() ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 243, 243, 243), OnTintColorPropertyChanged));

        /// <summary>
        /// Updates the UI when <see cref="TintColor"/> changes
        /// </summary>
        /// <param name="d">The current <see cref="BackdropMicaBrush"/> instance</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="TintColorProperty"/></param>
        private static void OnTintColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackdropMicaBrush brush = (BackdropMicaBrush)d;

            brush.TintToFallBackAnimation?.SetColorParameter("TintColor", (Color)e.NewValue);

            if (brush._isForce)
            {
                brush.CompositionBrush?.Properties.InsertColor("TintColor.Color", (Color)e.NewValue);
                brush.CompositionBrush?.Properties.InsertColor("LuminosityColor.Color", (Color)e.NewValue);
            }
            else
            {
                brush.Brush?.Properties.InsertColor("TintColor.Color", (Color)e.NewValue);
                brush.Brush?.Properties.InsertColor("LuminosityColor.Color", (Color)e.NewValue);
            }
        }

        /// <summary>
        /// Gets or sets the background source mode for the effect (the default is <see cref="BackgroundSource.Backdrop"/>).
        /// </summary>
        public BackgroundSource BackgroundSource
        {
            get => (BackgroundSource)GetValue(BackgroundSourceProperty);
            set => SetValue(BackgroundSourceProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="BackgroundSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BackgroundSourceProperty = DependencyProperty.Register(
            nameof(BackgroundSource),
            typeof(BackgroundSource),
            typeof(BackdropMicaBrush),
            new PropertyMetadata(BackgroundSource.MicaBackdrop, OnSourcePropertyChanged));

        /// <summary>
        /// Updates the UI when <see cref="BackgroundSource"/> changes
        /// </summary>
        /// <param name="d">The current <see cref="BackdropMicaBrush"/> instance</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="BackgroundSourceProperty"/></param>
        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackdropMicaBrush brush = (BackdropMicaBrush)d;

            brush.OnDisconnected();
            brush.OnConnected();
        }

        /// <summary>
        /// Gets or sets the tint opacity factor for the effect (default is 0.5, must be in the [0, 1] range)
        /// </summary>
        public double TintOpacity
        {
            get => (double)GetValue(TintOpacityProperty);
            set => SetValue(TintOpacityProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="TintOpacity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TintOpacityProperty = DependencyProperty.Register(
            nameof(TintOpacity),
            typeof(double),
            typeof(BackdropMicaBrush),
            new PropertyMetadata(UIHelper.IsDarkTheme() ? 0.8d : 0.5d, OnTintOpacityPropertyChanged));

        /// <summary>
        /// Updates the UI when <see cref="TintOpacity"/> changes
        /// </summary>
        /// <param name="d">The current <see cref="BackdropMicaBrush"/> instance</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="TintOpacityProperty"/></param>
        private static void OnTintOpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackdropMicaBrush brush = (BackdropMicaBrush)d;

            if ((double)e.NewValue > 1) { brush.TintOpacity = 1d; }
            else if ((double)e.NewValue < 0) { brush.TintOpacity = 0d; }
            brush.TintOpacityFillAnimation?.SetScalarParameter("TintOpacity", (float)(double)e.NewValue);

            if (brush._isForce)
            {
                brush.CompositionBrush?.Properties.InsertScalar("TintOpacity.Opacity", (float)(double)e.NewValue);
            }
            else
            {
                brush.Brush?.Properties.InsertScalar("TintOpacity.Opacity", (float)(double)e.NewValue);
            }
        }

        /// <summary>
        /// Gets or sets the tint opacity factor for the effect (default is 0.5, must be in the [0, 1] range)
        /// </summary>
        public double LuminosityOpacity
        {
            get => (double)GetValue(LuminosityOpacityProperty);
            set => SetValue(LuminosityOpacityProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="LuminosityOpacity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LuminosityOpacityProperty = DependencyProperty.Register(
            nameof(LuminosityOpacity),
            typeof(double),
            typeof(BackdropMicaBrush),
            new PropertyMetadata(1d, OnLuminosityOpacityPropertyChanged));

        /// <summary>
        /// Updates the UI when <see cref="LuminosityOpacity"/> changes
        /// </summary>
        /// <param name="d">The current <see cref="BackdropMicaBrush"/> instance</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="TintOpacityProperty"/></param>
        private static void OnLuminosityOpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackdropMicaBrush brush = (BackdropMicaBrush)d;

            if ((double)e.NewValue > 1) { brush.LuminosityOpacity = 1d; }
            else if ((double)e.NewValue < 0) { brush.LuminosityOpacity = 0d; }
            brush.HostOpacityZeroAnimation?.SetScalarParameter("LuminosityOpacity", (float)(double)e.NewValue);

            if (brush._isForce)
            {
                brush.CompositionBrush?.Properties.InsertScalar("LuminosityOpacity.Opacity", (float)(double)e.NewValue);
            }
            else
            {
                brush.Brush?.Properties.InsertScalar("LuminosityOpacity.Opacity", (float)(double)e.NewValue);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackdropMicaBrush"/> class.
        /// </summary>
        public BackdropMicaBrush()
        {
        }

        /// <summary>
        /// Initializes the Composition Brush.
        /// </summary>
        protected override void OnConnected()
        {
            // Delay creating composition resources until they're required.
            if (CompositionBrush == null)
            {
                // Abort if effects aren't supported.
                if (!CompositionCapabilities.GetForCurrentView().AreEffectsSupported())
                {
                    return;
                }

                ColorSourceEffect tintColorEffect = new ColorSourceEffect()
                {
                    Color = TintColor,
                    Name = "TintColor"
                };

                OpacityEffect tintOpacityEffect = new OpacityEffect()
                {
                    Name = "TintOpacity",
                    Source = tintColorEffect,
                    Opacity = (float)TintOpacity
                };

                ColorSourceEffect luminosityColorEffect = new ColorSourceEffect()
                {
                    Color = TintColor,
                    Name = "LuminosityColor"
                };

                OpacityEffect luminosityOpacityEffect = new OpacityEffect()
                {
                    Name = "LuminosityOpacity",
                    Source = luminosityColorEffect,
                    Opacity = (float)LuminosityOpacity
                };

                BlendEffect luminosityBlendEffect = new BlendEffect()
                {
                    Mode = BlendEffectMode.Color,
                    Foreground = luminosityOpacityEffect,
                    Background = new CompositionEffectSourceParameter("BlurredWallpaperBackdrop")
                };

                BlendEffect colorBlendEffect = new BlendEffect()
                {
                    Foreground = tintOpacityEffect,
                    Mode = BlendEffectMode.Luminosity,
                    Background = luminosityBlendEffect,
                };

                CompositionBackdropBrush backdrop;

                switch (BackgroundSource)
                {
                    case BackgroundSource.Backdrop:
                        if (!ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateBackdropBrush"))
                        {
                            CompositionBrush = Window.Current.Compositor.CreateColorBrush(FallbackColor);
                            return;
                        }
                        backdrop = Window.Current.Compositor.CreateBackdropBrush();
                        break;
                    case BackgroundSource.HostBackdrop:
                        if (!ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush"))
                        {
                            CompositionBrush = Window.Current.Compositor.CreateColorBrush(FallbackColor);
                            return;
                        }
                        backdrop = Window.Current.Compositor.CreateHostBackdropBrush();
                        break;
                    case BackgroundSource.MicaBackdrop:
                        if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush"))
                        {
                            backdrop = Window.Current.Compositor.TryCreateBlurredWallpaperBackdropBrush();
                        }
                        else if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush"))
                        {
                            backdrop = Window.Current.Compositor.CreateHostBackdropBrush();
                        }
                        else
                        {
                            CompositionBrush = Window.Current.Compositor.CreateColorBrush(FallbackColor);
                            return;
                        }
                        break;
                    default:
                        CompositionBrush = Window.Current.Compositor.CreateColorBrush(FallbackColor);
                        return;
                }

                CompositionEffectBrush micaEffectBrush = Window.Current.Compositor.CreateEffectFactory(colorBlendEffect, new[] { "TintColor.Color", "TintOpacity.Opacity", "LuminosityColor.Color", "LuminosityOpacity.Opacity" }).CreateBrush();
                micaEffectBrush.SetSourceParameter("BlurredWallpaperBackdrop", backdrop);

                Brush = micaEffectBrush;
                CompositionBrush = Brush;

                LinearEasingFunction line = Window.Current.Compositor.CreateLinearEasingFunction();

                TintOpacityFillAnimation = Window.Current.Compositor.CreateScalarKeyFrameAnimation();
                TintOpacityFillAnimation.InsertExpressionKeyFrame(0f, "TintOpacity", line);
                TintOpacityFillAnimation.InsertKeyFrame(1f, 1f, line);
                TintOpacityFillAnimation.Duration = TimeSpan.FromSeconds(0.1d);
                TintOpacityFillAnimation.Target = "TintOpacity.Opacity";

                HostOpacityZeroAnimation = Window.Current.Compositor.CreateScalarKeyFrameAnimation();
                HostOpacityZeroAnimation.InsertExpressionKeyFrame(0f, "LuminosityOpacity", line);
                HostOpacityZeroAnimation.InsertKeyFrame(1f, 1f, line);
                HostOpacityZeroAnimation.Duration = TimeSpan.FromSeconds(0.1d);
                HostOpacityZeroAnimation.Target = "LuminosityOpacity.Opacity";

                TintToFallBackAnimation = Window.Current.Compositor.CreateColorKeyFrameAnimation();
                TintToFallBackAnimation.InsertExpressionKeyFrame(0f, "TintColor", line);
                TintToFallBackAnimation.InsertExpressionKeyFrame(1f, "FallbackColor", line);
                TintToFallBackAnimation.Duration = TimeSpan.FromSeconds(0.1d);
                TintToFallBackAnimation.Target = "TintColor.Color";

                TintToFallBackAnimation?.SetColorParameter("TintColor", TintColor);
                TintOpacityFillAnimation?.SetScalarParameter("TintOpacity", (float)TintOpacity);
                HostOpacityZeroAnimation?.SetScalarParameter("LuminosityOpacity", (float)LuminosityOpacity);

                CoreWindow.GetForCurrentThread().Activated += CoreWindow_Activated;
                CoreWindow.GetForCurrentThread().VisibilityChanged += CoreWindow_VisibilityChanged;
                PowerManager.EnergySaverStatusChanged += On_EnergySaverStatusChanged;

                if (PowerManager.EnergySaverStatus == EnergySaverStatus.On)
                {
                    SetCompositionFocus(false);
                }
            }
        }

        /// <summary>
        /// Deconstructs the Composition Brush.
        /// </summary>
        protected override void OnDisconnected()
        {
            // Dispose of composition resources when no longer in use.
            if (CompositionBrush != null)
            {
                CompositionBrush.Dispose();
                CompositionBrush = null;
            }

            CoreWindow.GetForCurrentThread().Activated -= CoreWindow_Activated;
            CoreWindow.GetForCurrentThread().VisibilityChanged -= CoreWindow_VisibilityChanged;
            PowerManager.EnergySaverStatusChanged -= On_EnergySaverStatusChanged;
        }

        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            if (BackgroundSource == BackgroundSource.HostBackdrop || BackgroundSource == BackgroundSource.MicaBackdrop)
            {
                SetCompositionFocus(args.WindowActivationState != CoreWindowActivationState.Deactivated);
            }
        }

        private void CoreWindow_VisibilityChanged(CoreWindow sender, VisibilityChangedEventArgs args)
        {
            if (BackgroundSource == BackgroundSource.HostBackdrop || BackgroundSource == BackgroundSource.MicaBackdrop)
            {
                SetCompositionFocus(args.Visible);
            }
        }

        private async void On_EnergySaverStatusChanged(object sender, object e)
        {
            if (PowerManager.EnergySaverStatus == EnergySaverStatus.On)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    SetCompositionFocus(false);
                });
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (BackgroundSource == BackgroundSource.Backdrop)
                    {
                        SetCompositionFocus(true);
                    }
                    else
                    {
                        CoreWindow window = CoreWindow.GetForCurrentThread();
                        SetCompositionFocus(window.ActivationMode != CoreWindowActivationMode.Deactivated && window.Visible);
                    }
                });
            }
        }

        private void SetCompositionFocus(bool IsGotFocus)
        {
            IsGotFocus = IsGotFocus && PowerManager.EnergySaverStatus != EnergySaverStatus.On;
            if (CompositionBrush == null) { return; }
            if (BackgroundSource == BackgroundSource.Backdrop) { return; }
            TintToFallBackAnimation.SetColorParameter("FallbackColor", FallbackColor);
            if (IsGotFocus)
            {
                CompositionBrush = Brush;
                TintOpacityFillAnimation.Direction = AnimationDirection.Reverse;
                HostOpacityZeroAnimation.Direction = AnimationDirection.Reverse;
                TintToFallBackAnimation.Direction = AnimationDirection.Reverse;
                CompositionBrush.StartAnimation("TintOpacity.Opacity", TintOpacityFillAnimation);
                CompositionBrush.StartAnimation("LuminosityOpacity.Opacity", HostOpacityZeroAnimation);
                CompositionBrush.StartAnimation("TintColor.Color", TintToFallBackAnimation);
            }
            else if (CompositionBrush == Brush)
            {
                CompositionScopedBatch scopedBatch = Window.Current.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                TintOpacityFillAnimation.Direction = AnimationDirection.Normal;
                HostOpacityZeroAnimation.Direction = AnimationDirection.Normal;
                TintToFallBackAnimation.Direction = AnimationDirection.Normal;
                CompositionBrush.StartAnimation("TintOpacity.Opacity", TintOpacityFillAnimation);
                CompositionBrush.StartAnimation("LuminosityOpacity.Opacity", HostOpacityZeroAnimation);
                CompositionBrush.StartAnimation("TintColor.Color", TintToFallBackAnimation);
                scopedBatch.Completed += (s, a) => CompositionBrush = Window.Current.Compositor.CreateColorBrush(FallbackColor);
                scopedBatch.End();
            }
            else
            {
                CompositionBrush = Window.Current.Compositor.CreateColorBrush(FallbackColor);
            }
            _isForce = IsGotFocus;
        }
    }
}
