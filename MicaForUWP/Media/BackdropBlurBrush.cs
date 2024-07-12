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
    /// The <see cref="BackdropBlurBrush"/> is a <see cref="Brush"/> that blurs whatever is behind it in the application.
    /// </summary>
    [ContractVersion(typeof(UniversalApiContract), 262144)]
#if WINRT
    sealed
#endif
    public class BackdropBlurBrush : XamlCompositionBrushBase
    {
        private bool _isForce = true;

        private CompositionBrush brush;
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
        public static DependencyProperty AlwaysUseFallbackProperty { get; } =
            DependencyProperty.Register(
                nameof(AlwaysUseFallback),
                typeof(bool),
                typeof(BackdropBlurBrush),
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
                typeof(BackdropBlurBrush),
                new PropertyMetadata(BackgroundSource.Backdrop, OnSourcePropertyChanged));

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
        /// <param name="d">The current <see cref="AcrylicBrush"/> instance</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="BackgroundSourceProperty"/></param>
        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BackdropBlurBrush brush && e.NewValue?.Equals(e.OldValue) != true)
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
                typeof(BackdropBlurBrush),
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
        /// <param name="d">The current <see cref="AcrylicBrush"/> instance</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="TintColorProperty"/></param>
        private static void OnTintColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BackdropBlurBrush brush && e.NewValue?.Equals(e.OldValue) != true)
            {
                brush.tintToFallBackAnimation?.SetColorParameter("TintColor", (Color)e.NewValue);
                if (brush._isForce)
                {
                    brush.CompositionBrush?.Properties.InsertColor("TintColor.Color", (Color)e.NewValue);
                }
                else
                {
                    brush.brush?.Properties.InsertColor("TintColor.Color", (Color)e.NewValue);
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
                typeof(BackdropBlurBrush),
                new PropertyMetadata(30d, new PropertyChangedCallback(OnAmountChanged)));

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
            if (d is BackdropBlurBrush brush && e.NewValue?.Equals(e.OldValue) != true)
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
                if (brush._isForce)
                {
                    brush.CompositionBrush?.Properties.InsertScalar("Blur.BlurAmount", (float)value);
                }
                else
                {
                    brush.brush?.Properties.InsertScalar("Blur.BlurAmount", (float)value);
                }
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
                typeof(BackdropBlurBrush),
                new PropertyMetadata(1d, OnTintOpacityPropertyChanged));

        /// <summary>
        /// Gets or sets the tint opacity factor for the effect (default is 1.0, must be in the [0, 1] range)
        /// </summary>
        public double TintOpacity
        {
            get => (double)GetValue(TintOpacityProperty);
            set => SetValue(TintOpacityProperty, value);
        }

        /// <summary>
        /// Updates the UI when <see cref="TintOpacity"/> changes
        /// </summary>
        /// <param name="d">The current <see cref="AcrylicBrush"/> instance</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="TintOpacityProperty"/></param>
        private static void OnTintOpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BackdropBlurBrush brush && e.NewValue?.Equals(e.OldValue) != true)
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

                brush.hostOpacityZeroAnimation?.SetScalarParameter("TintOpacity", (float)value);
                brush.tintOpacityFillAnimation?.SetScalarParameter("TintOpacity", (float)value);

                if (brush._isForce)
                {
                    brush.CompositionBrush?.Properties.InsertScalar("Arithmetic.Source1Amount", (float)(1 - value));
                    brush.CompositionBrush?.Properties.InsertScalar("Arithmetic.Source2Amount", (float)value);
                }
                else
                {
                    brush.brush?.Properties.InsertScalar("Arithmetic.Source1Amount", (float)(1 - value));
                    brush.brush?.Properties.InsertScalar("Arithmetic.Source2Amount", (float)value);
                }
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
                typeof(BackdropBlurBrush),
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
        /// Initializes a new instance of the <see cref="BackdropBlurBrush"/> class.
        /// </summary>
        public BackdropBlurBrush() { }

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

                if (!AlwaysUseFallback)
                {
                    CompositionBackdropBrush backdrop;

                    switch (BackgroundSource)
                    {
                        case BackgroundSource.Backdrop:
                            if (!ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateBackdropBrush"))
                            {
                                CompositionBrush = compositor.CreateColorBrush(FallbackColor);
                                return;
                            }
                            backdrop = compositor.CreateBackdropBrush();
                            break;
                        case BackgroundSource.HostBackdrop:
                            if (!ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush"))
                            {
                                CompositionBrush = compositor.CreateColorBrush(FallbackColor);
                                return;
                            }
                            backdrop = compositor.CreateHostBackdropBrush();
                            break;
                        case BackgroundSource.WallpaperBackdrop:
                            if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush"))
                            {
                                backdrop = compositor.TryCreateBlurredWallpaperBackdropBrush();
                            }
                            else if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush"))
                            {
                                backdrop = compositor.CreateHostBackdropBrush();
                            }
                            else
                            {
                                CompositionBrush = compositor.CreateColorBrush(FallbackColor);
                                return;
                            }
                            break;
                        default:
                            CompositionBrush = compositor.CreateColorBrush(FallbackColor);
                            return;
                    }

                    // Use a Win2D blur affect applied to a CompositionBackdropBrush.
                    ArithmeticCompositeEffect compositeEffect = new ArithmeticCompositeEffect
                    {
                        Name = "Arithmetic",
                        MultiplyAmount = 0f,
                        Source1Amount = (float)(1f - TintOpacity),
                        Source2Amount = (float)TintOpacity,
                        Source1 = new CompositeEffect
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
                                    Source = new CompositionEffectSourceParameter("backdrop")
                                }
                            }
                        },
                        Source2 = new ColorSourceEffect
                        {
                            Name = "TintColor",
                            Color = TintColor
                        }
                    };

                    CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(compositeEffect, new[] { "Blur.BlurAmount", "Arithmetic.Source1Amount", "Arithmetic.Source2Amount", "TintColor.Color" });
                    CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

                    effectBrush.SetSourceParameter("backdrop", backdrop);

                    brush = effectBrush;
                    CompositionBrush = brush;

                    LinearEasingFunction line = compositor.CreateLinearEasingFunction();

                    TimeSpan duration = TintTransitionDuration == TimeSpan.Zero ? TimeSpan.FromTicks(10000) : TintTransitionDuration;
                    TimeSpan switchDuration = TimeSpan.FromMilliseconds(167);

                    tintOpacityFillAnimation = compositor.CreateScalarKeyFrameAnimation();
                    tintOpacityFillAnimation.InsertExpressionKeyFrame(0f, "TintOpacity", line);
                    tintOpacityFillAnimation.InsertKeyFrame(1f, 1f, line);
                    tintOpacityFillAnimation.Duration = switchDuration;
                    tintOpacityFillAnimation.Target = "Arithmetic.Source2Amount";

                    hostOpacityZeroAnimation = compositor.CreateScalarKeyFrameAnimation();
                    hostOpacityZeroAnimation.InsertExpressionKeyFrame(0f, "1f - TintOpacity", line);
                    hostOpacityZeroAnimation.InsertKeyFrame(1f, 0f, line);
                    hostOpacityZeroAnimation.Duration = switchDuration;
                    hostOpacityZeroAnimation.Target = "Arithmetic.Source1Amount";

                    tintToFallBackAnimation = compositor.CreateColorKeyFrameAnimation();
                    tintToFallBackAnimation.InsertExpressionKeyFrame(0f, "TintColor", line);
                    tintToFallBackAnimation.InsertExpressionKeyFrame(1f, "FallbackColor", line);
                    tintToFallBackAnimation.Duration = duration;
                    tintToFallBackAnimation.Target = "TintColor.Color";

                    tintToFallBackAnimation?.SetColorParameter("TintColor", TintColor);
                    hostOpacityZeroAnimation?.SetScalarParameter("TintOpacity", (float)TintOpacity);
                    tintOpacityFillAnimation?.SetScalarParameter("TintOpacity", (float)TintOpacity);

                    CoreWindow window = CoreWindow.GetForCurrentThread();
                    window.Activated += CoreWindow_Activated;
                    window.VisibilityChanged += CoreWindow_VisibilityChanged;
                    PowerManager.EnergySaverStatusChanged += On_EnergySaverStatusChanged;
                    
                    if (PowerManager.EnergySaverStatus == EnergySaverStatus.On)
                    {
                        SetCompositionFocus(false);
                    }
                }
                else
                {
                    brush = compositor.CreateColorBrush(FallbackColor);
                    CompositionBrush = brush;
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
            tintToFallBackAnimation.SetColorParameter("FallbackColor", FallbackColor);
            if (IsGotFocus)
            {
                CompositionBrush = brush;
                tintOpacityFillAnimation.Direction = AnimationDirection.Reverse;
                hostOpacityZeroAnimation.Direction = AnimationDirection.Reverse;
                tintToFallBackAnimation.Direction = AnimationDirection.Reverse;
                CompositionBrush.StartAnimation("Arithmetic.Source2Amount", tintOpacityFillAnimation);
                CompositionBrush.StartAnimation("Arithmetic.Source1Amount", hostOpacityZeroAnimation);
                CompositionBrush.StartAnimation("TintColor.Color", tintToFallBackAnimation);
            }
            else if (CompositionBrush == brush)
            {
                if (!(Compositor is Compositor compositor)) { return; }
                CompositionScopedBatch scopedBatch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                tintOpacityFillAnimation.Direction = AnimationDirection.Normal;
                hostOpacityZeroAnimation.Direction = AnimationDirection.Normal;
                tintToFallBackAnimation.Direction = AnimationDirection.Normal;
                CompositionBrush.StartAnimation("Arithmetic.Source2Amount", tintOpacityFillAnimation);
                CompositionBrush.StartAnimation("Arithmetic.Source1Amount", hostOpacityZeroAnimation);
                CompositionBrush.StartAnimation("TintColor.Color", tintToFallBackAnimation);
                scopedBatch.Completed += (s, a) => CompositionBrush = compositor.CreateColorBrush(FallbackColor);
                scopedBatch.End();
            }
            else
            {
                if (!(Compositor is Compositor compositor)) { return; }
                CompositionBrush = compositor.CreateColorBrush(FallbackColor);
            }
            _isForce = IsGotFocus;
        }
    }
}
