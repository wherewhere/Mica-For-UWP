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
#if WINRT
    sealed
#endif
    public class BackdropMicaBrush : XamlCompositionBrushBase
    {
        private bool _isForce = true;

        private Compositor Compositor;
        private CompositionBrush Brush;
        private ScalarKeyFrameAnimation TintOpacityFillAnimation;
        private ScalarKeyFrameAnimation HostOpacityZeroAnimation;
        private ColorKeyFrameAnimation TintToFallBackAnimation;

        #region AlwaysUseFallback

        /// <summary>
        /// Identifies the <see cref="AlwaysUseFallback"/> dependency property.
        /// </summary>
        private static DependencyProperty AlwaysUseFallbackProperty { get; } =
            DependencyProperty.Register(
                nameof(AlwaysUseFallback),
                typeof(bool),
                typeof(BackdropMicaBrush),
                new PropertyMetadata(false, new PropertyChangedCallback(OnSourcePropertyChanged)));

        /// <summary>
        /// Gets or sets a value that specifies whether the brush is forced to the solid fallback color.
        /// </summary>
        public bool AlwaysUseFallback
        {
            get => (bool)GetValue(AlwaysUseFallbackProperty);
            set => SetValue(AlwaysUseFallbackProperty, value);
        }

        #endregion

        #region BackgroundSource

        /// <summary>
        /// Identifies the <see cref="BackgroundSource"/> dependency property.
        /// </summary>
        public static DependencyProperty BackgroundSourceProperty { get; } =
            DependencyProperty.Register(
                nameof(BackgroundSource),
                typeof(BackgroundSource),
                typeof(BackdropMicaBrush),
                new PropertyMetadata(BackgroundSource.WallpaperBackdrop, OnSourcePropertyChanged));

        /// <summary>
        /// Gets or sets the background source mode for the effect (the default is <see cref="BackgroundSource.Backdrop"/>).
        /// </summary>
        public BackgroundSource BackgroundSource
        {
            get => (BackgroundSource)GetValue(BackgroundSourceProperty);
            set => SetValue(BackgroundSourceProperty, value);
        }

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

        #endregion

        #region TintColor

        /// <summary>
        /// Identifies the <see cref="TintColor"/> dependency property.
        /// </summary>
        public static DependencyProperty TintColorProperty { get; } =
            DependencyProperty.Register(
                nameof(TintColor),
                typeof(Color),
                typeof(BackdropMicaBrush),
                new PropertyMetadata(default(Color), OnTintColorPropertyChanged));

        /// <summary>
        /// Gets or sets the tint for the effect
        /// </summary>
        public Color TintColor
        {
            get => (Color)GetValue(TintColorProperty);
            set => SetValue(TintColorProperty, value);
        }

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

        #endregion

        #region LuminosityOpacity

        /// <summary>
        /// Identifies the <see cref="LuminosityOpacity"/> dependency property.
        /// </summary>
        public static DependencyProperty LuminosityOpacityProperty { get; } =
            DependencyProperty.Register(
                nameof(LuminosityOpacity),
                typeof(double),
                typeof(BackdropMicaBrush),
                new PropertyMetadata(1d, OnLuminosityOpacityPropertyChanged));

        /// <summary>
        /// Gets or sets the tint opacity factor for the effect (default is 0.5, must be in the [0, 1] range)
        /// </summary>
        public double LuminosityOpacity
        {
            get => (double)GetValue(LuminosityOpacityProperty);
            set => SetValue(LuminosityOpacityProperty, value);
        }

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

        #endregion

        #region TintOpacity

        /// <summary>
        /// Identifies the <see cref="TintOpacity"/> dependency property.
        /// </summary>
        public static DependencyProperty TintOpacityProperty { get; } =
            DependencyProperty.Register(
                nameof(TintOpacity),
                typeof(double),
                typeof(BackdropMicaBrush),
                new PropertyMetadata(0.8d, OnTintOpacityPropertyChanged));

        /// <summary>
        /// Gets or sets the tint opacity factor for the effect (default is 0.8, must be in the [0, 1] range)
        /// </summary>
        public double TintOpacity
        {
            get => (double)GetValue(TintOpacityProperty);
            set => SetValue(TintOpacityProperty, value);
        }

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

        #endregion

        #region TintTransitionDuration

        /// <summary>
        /// Identifies the <see cref="TintTransitionDuration"/> dependency property.
        /// </summary>
        public static DependencyProperty TintTransitionDurationProperty { get; } =
            DependencyProperty.Register(
                nameof(TintTransitionDuration),
                typeof(TimeSpan),
                typeof(BackdropMicaBrush),
                new PropertyMetadata(TimeSpan.FromMilliseconds(500)));

        /// <summary>
        /// Gets or sets the length of the automatic transition animation used when the TintColor changes.
        /// </summary>
        public TimeSpan TintTransitionDuration
        {
            get => (TimeSpan)GetValue(TintTransitionDurationProperty);
            set => SetValue(TintTransitionDurationProperty, value);
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="BackdropMicaBrush"/> class.
        /// </summary>
        public BackdropMicaBrush() { }

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

                if (Window.Current != null)
                {
                    Compositor = Window.Current.Compositor;
                }

                if (Compositor == null) { return; }

                if (!AlwaysUseFallback)
                {
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
                                CompositionBrush = Compositor.CreateColorBrush(FallbackColor);
                                return;
                            }
                            backdrop = Compositor.CreateBackdropBrush();
                            break;
                        case BackgroundSource.HostBackdrop:
                            if (!ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush"))
                            {
                                CompositionBrush = Compositor.CreateColorBrush(FallbackColor);
                                return;
                            }
                            backdrop = Compositor.CreateHostBackdropBrush();
                            break;
                        case BackgroundSource.WallpaperBackdrop:
                            if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush"))
                            {
                                backdrop = Compositor.TryCreateBlurredWallpaperBackdropBrush();
                            }
                            else if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush"))
                            {
                                backdrop = Compositor.CreateHostBackdropBrush();
                            }
                            else
                            {
                                CompositionBrush = Compositor.CreateColorBrush(FallbackColor);
                                return;
                            }
                            break;
                        default:
                            CompositionBrush = Compositor.CreateColorBrush(FallbackColor);
                            return;
                    }

                    CompositionEffectBrush micaEffectBrush = Compositor.CreateEffectFactory(colorBlendEffect, new[] { "TintColor.Color", "TintOpacity.Opacity", "LuminosityColor.Color", "LuminosityOpacity.Opacity" }).CreateBrush();
                    micaEffectBrush.SetSourceParameter("BlurredWallpaperBackdrop", backdrop);

                    Brush = micaEffectBrush;
                    CompositionBrush = Brush;

                    LinearEasingFunction line = Compositor.CreateLinearEasingFunction();

                    TimeSpan duration = TintTransitionDuration == TimeSpan.Zero ? TimeSpan.FromTicks(10000) : TintTransitionDuration;
                    TimeSpan switchDuration = TimeSpan.FromMilliseconds(167);

                    TintOpacityFillAnimation = Compositor.CreateScalarKeyFrameAnimation();
                    TintOpacityFillAnimation.InsertExpressionKeyFrame(0f, "TintOpacity", line);
                    TintOpacityFillAnimation.InsertKeyFrame(1f, 1f, line);
                    TintOpacityFillAnimation.Duration = switchDuration;
                    TintOpacityFillAnimation.Target = "TintOpacity.Opacity";

                    HostOpacityZeroAnimation = Compositor.CreateScalarKeyFrameAnimation();
                    HostOpacityZeroAnimation.InsertExpressionKeyFrame(0f, "LuminosityOpacity", line);
                    HostOpacityZeroAnimation.InsertKeyFrame(1f, 1f, line);
                    HostOpacityZeroAnimation.Duration = switchDuration;
                    HostOpacityZeroAnimation.Target = "LuminosityOpacity.Opacity";

                    TintToFallBackAnimation = Compositor.CreateColorKeyFrameAnimation();
                    TintToFallBackAnimation.InsertExpressionKeyFrame(0f, "TintColor", line);
                    TintToFallBackAnimation.InsertExpressionKeyFrame(1f, "FallbackColor", line);
                    TintToFallBackAnimation.Duration = duration;
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
                else
                {
                    Brush = Compositor.CreateColorBrush(FallbackColor);
                    CompositionBrush = Brush;
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
            if (BackgroundSource == BackgroundSource.HostBackdrop || BackgroundSource == BackgroundSource.WallpaperBackdrop)
            {
                SetCompositionFocus(args.WindowActivationState != CoreWindowActivationState.Deactivated);
            }
        }

        private void CoreWindow_VisibilityChanged(CoreWindow sender, VisibilityChangedEventArgs args)
        {
            if (BackgroundSource == BackgroundSource.HostBackdrop || BackgroundSource == BackgroundSource.WallpaperBackdrop)
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
                if (Window.Current != null)
                {
                    Compositor = Window.Current.Compositor;
                }

                if (Compositor == null) { return; }

                CompositionScopedBatch scopedBatch = Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                TintOpacityFillAnimation.Direction = AnimationDirection.Normal;
                HostOpacityZeroAnimation.Direction = AnimationDirection.Normal;
                TintToFallBackAnimation.Direction = AnimationDirection.Normal;
                CompositionBrush.StartAnimation("TintOpacity.Opacity", TintOpacityFillAnimation);
                CompositionBrush.StartAnimation("LuminosityOpacity.Opacity", HostOpacityZeroAnimation);
                CompositionBrush.StartAnimation("TintColor.Color", TintToFallBackAnimation);
                scopedBatch.Completed += (s, a) => CompositionBrush = Compositor.CreateColorBrush(FallbackColor);
                scopedBatch.End();
            }
            else
            {
                if (Window.Current != null)
                {
                    Compositor = Window.Current.Compositor;
                }

                if (Compositor == null) { return; }

                CompositionBrush = Compositor.CreateColorBrush(FallbackColor);
            }
            _isForce = IsGotFocus;
        }
    }
}
