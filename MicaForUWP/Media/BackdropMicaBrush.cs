using MicaForUWP.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Diagnostics;
using System.Runtime.Versioning;
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
#if NET
    [SupportedOSPlatform("Windows10.0.15063.0")]
#endif
    [ContractVersion(typeof(UniversalApiContract), 0x40000)]
#if WINRT
    sealed
#endif
    public partial class BackdropMicaBrush : XamlCompositionBrushBase
    {
        private bool _isForce = true;

        private CompositionEffectBrush brush;
        private CompositionColorBrush fallback;
        private ScalarKeyFrameAnimation tintOpacityFillAnimation;
        private ScalarKeyFrameAnimation hostOpacityZeroAnimation;
        private ColorKeyFrameAnimation tintToFallBackAnimation;

        private Compositor compositor;
        private Compositor Compositor
        {
            get
            {
                if (compositor == null && Window.Current is Window window)
                {
                    compositor = window.Compositor;
                }
                return compositor;
            }
        }

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
            if (d is BackdropMicaBrush brush && e.NewValue?.Equals(e.OldValue) != true)
            {
                brush.OnDisconnected();
                brush.OnConnected();
            }
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
            if (d is BackdropMicaBrush brush && e.NewValue?.Equals(e.OldValue) != true)
            {
                Color color = (Color)e.NewValue;
                brush.tintToFallBackAnimation?.SetColorParameter("TintColor", color);

                if (brush.brush is CompositionEffectBrush effect)
                {
                    effect.Properties.InsertColor("TintColor.Color", color);
                    effect.Properties.InsertColor("LuminosityColor.Color", color);
                }

                if (brush.fallback is CompositionColorBrush fallback)
                {
                    fallback.Color = color;
                }
            }
        }

        #endregion

        #region Amount

        /// <summary>
        /// Identifies the <see cref="Amount"/> dependency property.
        /// </summary>
        public static DependencyProperty AmountProperty { get; } =
            DependencyProperty.Register(
                nameof(Amount),
                typeof(double),
                typeof(BackdropMicaBrush),
                new PropertyMetadata(0d, new PropertyChangedCallback(OnAmountChanged)));

        /// <summary>
        /// Gets or sets the amount of gaussian blur to apply to the background.
        /// </summary>
        public double Amount
        {
            get => (double)GetValue(AmountProperty);
            set => SetValue(AmountProperty, value);
        }

        private static void OnAmountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BackdropMicaBrush brush && e.NewValue?.Equals(e.OldValue) != true)
            {
                double value = (double)e.NewValue;
                if (value > 100)
                {
                    brush.Amount = value = 100;
                }
                else if (value < 0)
                {
                    brush.Amount = value = 0;
                }

                // Unbox and set a new blur amount if the CompositionBrush exists.
                brush.brush?.Properties.InsertScalar("Blur.BlurAmount", (float)value);
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
            if (d is BackdropMicaBrush brush && e.NewValue?.Equals(e.OldValue) != true)
            {
                double value = (double)e.NewValue;
                if (value > 1)
                {
                    brush.LuminosityOpacity = value = 1;
                }
                else if (value < 0)
                {
                    brush.LuminosityOpacity = value = 0;
                }
                brush.hostOpacityZeroAnimation?.SetScalarParameter("LuminosityOpacity", (float)value);
                brush.brush?.Properties.InsertScalar("LuminosityOpacity.Opacity", (float)value);
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
            if (d is BackdropMicaBrush brush && e.NewValue?.Equals(e.OldValue) != true)
            {
                double value = (double)e.NewValue;
                if (value > 1)
                {
                    brush.TintOpacity = value = 1;
                }
                else if (value < 0)
                {
                    brush.TintOpacity = value = 0;
                }
                brush.tintOpacityFillAnimation?.SetScalarParameter("TintOpacity", (float)value);
                brush.brush?.Properties.InsertScalar("TintOpacity.Opacity", (float)value);
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
                if (!(CompositionCapabilities.GetForCurrentView().AreEffectsSupported()
                    && Compositor is Compositor compositor))
                {
                    return;
                }

                fallback = compositor.CreateColorBrush(FallbackColor);

                if (!AlwaysUseFallback)
                {
                    try
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
                            Background = new CompositeEffect
                            {
                                Sources =
                                {
                                    new ColorSourceEffect
                                    {
                                        Color = Colors.Black
                                    },
                                    new GaussianBlurEffect
                                    {
                                        Name = "Blur",
                                        BlurAmount = (float)Amount,
                                        BorderMode = EffectBorderMode.Hard,
                                        Optimization = EffectOptimization.Balanced,
                                        Source = new CompositionEffectSourceParameter("BlurredWallpaperBackdrop")
                                    }
                                }
                            }
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
                                if (ApiInfoHelper.IsCreateBackdropBrushSupported)
                                {
                                    backdrop = compositor.CreateBackdropBrush();
                                }
                                else
                                {
                                    goto fallback;
                                }
                                break;
                            case BackgroundSource.HostBackdrop:
                                if (ApiInfoHelper.IsCreateHostBackdropBrushSupported)
                                {
                                    backdrop = compositor.CreateHostBackdropBrush();
                                }
                                else
                                {
                                    goto fallback;
                                }
                                break;
                            case BackgroundSource.WallpaperBackdrop:
                                if (ApiInfoHelper.IsTryCreateBlurredWallpaperBackdropBrushSupported)
                                {
                                    backdrop = compositor.TryCreateBlurredWallpaperBackdropBrush();
                                }
                                else if (ApiInfoHelper.IsCreateHostBackdropBrushSupported)
                                {
                                    backdrop = compositor.CreateHostBackdropBrush();
                                }
                                else
                                {
                                    goto fallback;
                                }
                                break;
                            default:
                                goto fallback;
                        }

                        CompositionEffectBrush micaEffectBrush = compositor.CreateEffectFactory(colorBlendEffect, new[] { "TintColor.Color", "TintOpacity.Opacity", "LuminosityColor.Color", "LuminosityOpacity.Opacity", "Blur.BlurAmount" }).CreateBrush();
                        micaEffectBrush.SetSourceParameter("BlurredWallpaperBackdrop", backdrop);

                        brush = micaEffectBrush;
                        CompositionBrush = brush;

                        LinearEasingFunction line = compositor.CreateLinearEasingFunction();

                        TimeSpan duration = TintTransitionDuration == TimeSpan.Zero ? TimeSpan.FromTicks(10000) : TintTransitionDuration;
                        TimeSpan switchDuration = TimeSpan.FromMilliseconds(167);

                        tintOpacityFillAnimation = compositor.CreateScalarKeyFrameAnimation();
                        tintOpacityFillAnimation.InsertExpressionKeyFrame(0f, "TintOpacity", line);
                        tintOpacityFillAnimation.InsertKeyFrame(1f, 1f, line);
                        tintOpacityFillAnimation.Duration = switchDuration;
                        tintOpacityFillAnimation.Target = "TintOpacity.Opacity";

                        hostOpacityZeroAnimation = compositor.CreateScalarKeyFrameAnimation();
                        hostOpacityZeroAnimation.InsertExpressionKeyFrame(0f, "LuminosityOpacity", line);
                        hostOpacityZeroAnimation.InsertKeyFrame(1f, 1f, line);
                        hostOpacityZeroAnimation.Duration = switchDuration;
                        hostOpacityZeroAnimation.Target = "LuminosityOpacity.Opacity";

                        tintToFallBackAnimation = compositor.CreateColorKeyFrameAnimation();
                        tintToFallBackAnimation.InsertExpressionKeyFrame(0f, "TintColor", line);
                        tintToFallBackAnimation.InsertExpressionKeyFrame(1f, "FallbackColor", line);
                        tintToFallBackAnimation.Duration = duration;
                        tintToFallBackAnimation.Target = "TintColor.Color";

                        tintToFallBackAnimation?.SetColorParameter("TintColor", TintColor);
                        tintOpacityFillAnimation?.SetScalarParameter("TintOpacity", (float)TintOpacity);
                        hostOpacityZeroAnimation?.SetScalarParameter("LuminosityOpacity", (float)LuminosityOpacity);

                        CoreWindow window = CoreWindow.GetForCurrentThread();
                        window.Activated += CoreWindow_Activated;
                        window.VisibilityChanged += CoreWindow_VisibilityChanged;
                        PowerManager.EnergySaverStatusChanged += On_EnergySaverStatusChanged;

                        if (PowerManager.EnergySaverStatus == EnergySaverStatus.On)
                        {
                            SetCompositionFocus(false);
                        }

                        return;
                    }
                    catch (Exception ex)
                    {
                        Debug.Fail(ex.ToString());
                    }
                }

            fallback:
                brush = null;
                CompositionBrush = fallback;
            }
        }

        /// <summary>
        /// Deconstructs the Composition Brush.
        /// </summary>
        protected override void OnDisconnected()
        {
            CompositionBrush = null;

            // Dispose of composition resources when no longer in use.
            if (brush != null)
            {
                brush.Dispose();
                brush = null;
            }

            if (fallback != null)
            {
                fallback.Dispose();
                fallback = null;
            }

            CoreWindow window = CoreWindow.GetForCurrentThread();
            window.Activated -= CoreWindow_Activated;
            window.VisibilityChanged -= CoreWindow_VisibilityChanged;
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
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SetCompositionFocus(false));
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
                        SetCompositionFocus(
                            ApiInfoHelper.IsActivationModeSupported
                            ? window.ActivationMode != CoreWindowActivationMode.Deactivated && window.Visible
                            : window.Visible);
                    }
                });
            }
        }

        private void SetCompositionFocus(bool isGotFocus)
        {
            if (BackgroundSource == BackgroundSource.Backdrop
                || CompositionBrush == null
                || brush == null) { return; }
            tintToFallBackAnimation.SetColorParameter("FallbackColor", FallbackColor);
            if (_isForce = isGotFocus && PowerManager.EnergySaverStatus != EnergySaverStatus.On)
            {
                CompositionBrush = brush;
                tintOpacityFillAnimation.Direction = AnimationDirection.Reverse;
                hostOpacityZeroAnimation.Direction = AnimationDirection.Reverse;
                tintToFallBackAnimation.Direction = AnimationDirection.Reverse;
                brush.StartAnimation("TintOpacity.Opacity", tintOpacityFillAnimation);
                brush.StartAnimation("LuminosityOpacity.Opacity", hostOpacityZeroAnimation);
                brush.StartAnimation("TintColor.Color", tintToFallBackAnimation);
            }
            else if (CompositionBrush == brush)
            {
                if (!(Compositor is Compositor compositor)) { return; }
                CompositionScopedBatch scopedBatch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                tintOpacityFillAnimation.Direction = AnimationDirection.Normal;
                hostOpacityZeroAnimation.Direction = AnimationDirection.Normal;
                tintToFallBackAnimation.Direction = AnimationDirection.Normal;
                brush.StartAnimation("TintOpacity.Opacity", tintOpacityFillAnimation);
                brush.StartAnimation("LuminosityOpacity.Opacity", hostOpacityZeroAnimation);
                brush.StartAnimation("TintColor.Color", tintToFallBackAnimation);
                scopedBatch.Completed += (s, a) => { if (!_isForce) { CompositionBrush = fallback; } };
                scopedBatch.End();
            }
        }
    }
}
