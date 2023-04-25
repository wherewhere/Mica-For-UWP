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

        private Compositor Compositor;
        private CompositionBrush Brush;
        private ScalarKeyFrameAnimation TintOpacityFillAnimation;
        private ScalarKeyFrameAnimation HostOpacityZeroAnimation;
        private ColorKeyFrameAnimation TintToFallBackAnimation;

        #region AlwaysUseFallback

        /// <summary>
        /// Identifies the <see cref="AlwaysUseFallback"/> dependency property.
        /// </summary>
#if !WINRT
        readonly
#endif
        public static DependencyProperty AlwaysUseFallbackProperty
#if WINRT
            => _alwaysUseFallbackProperty;

        private static readonly DependencyProperty _alwaysUseFallbackProperty
#endif
            = DependencyProperty.Register(
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
#if !WINRT
        readonly
#endif
        public static DependencyProperty BackgroundSourceProperty
#if WINRT
            => _backgroundSourceProperty;

        private static readonly DependencyProperty _backgroundSourceProperty
#endif
            = DependencyProperty.Register(
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
            BackdropBlurBrush brush = (BackdropBlurBrush)d;

            brush.OnDisconnected();
            brush.OnConnected();
        }

        #endregion

        #region TintColor

        /// <summary>
        /// Identifies the <see cref="TintColor"/> dependency property.
        /// </summary>
#if !WINRT
        readonly
#endif
        public static DependencyProperty TintColorProperty
#if WINRT
            => _tintColorProperty;

        private static readonly DependencyProperty _tintColorProperty
#endif
            = DependencyProperty.Register(
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
            BackdropBlurBrush brush = (BackdropBlurBrush)d;

            brush.TintToFallBackAnimation?.SetColorParameter("TintColor", (Color)e.NewValue);

            if (brush._isForce)
            {
                brush.CompositionBrush?.Properties.InsertColor("TintColor.Color", (Color)e.NewValue);
            }
            else
            {
                brush.Brush?.Properties.InsertColor("TintColor.Color", (Color)e.NewValue);
            }
        }

        #endregion

        #region Amount

        /// <summary>
        /// Identifies the <see cref="Amount"/> dependency property.
        /// </summary>
#if !WINRT
        readonly
#endif
        public static DependencyProperty AmountProperty
#if WINRT
            => _amountProperty;

        private static readonly DependencyProperty _amountProperty
#endif
            = DependencyProperty.Register(
                nameof(Amount),
                typeof(double),
                typeof(BackdropBlurBrush),
                new PropertyMetadata(30.0d, new PropertyChangedCallback(OnAmountChanged)));

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
            BackdropBlurBrush brush = (BackdropBlurBrush)d;

            if ((double)e.NewValue > 100) { brush.Amount = 100d; }
            else if ((double)e.NewValue < 0) { brush.Amount = 0d; }
            // Unbox and set a new blur amount if the CompositionBrush exists.
            if (brush._isForce)
            {
                brush.CompositionBrush?.Properties.InsertScalar("Blur.BlurAmount", (float)(double)e.NewValue);
            }
            else
            {
                brush.Brush?.Properties.InsertScalar("Blur.BlurAmount", (float)(double)e.NewValue);
            }
        }

        #endregion

        #region TintOpacity

        /// <summary>
        /// Identifies the <see cref="TintOpacity"/> dependency property.
        /// </summary>
#if !WINRT
        readonly
#endif
        public static DependencyProperty TintOpacityProperty
#if WINRT
            => _tintOpacityProperty;

        private static readonly DependencyProperty _tintOpacityProperty
#endif
            = DependencyProperty.Register(
                nameof(TintOpacity),
                typeof(double),
                typeof(BackdropBlurBrush),
                new PropertyMetadata(1.0d, OnTintOpacityPropertyChanged));

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
            BackdropBlurBrush brush = (BackdropBlurBrush)d;

            if ((double)e.NewValue > 1) { brush.TintOpacity = 1d; }
            else if ((double)e.NewValue < 0) { brush.TintOpacity = 0d; }
            brush.HostOpacityZeroAnimation?.SetScalarParameter("TintOpacity", (float)(double)e.NewValue);
            brush.TintOpacityFillAnimation?.SetScalarParameter("TintOpacity", (float)(double)e.NewValue);

            if (brush._isForce)
            {
                brush.CompositionBrush?.Properties.InsertScalar("Arithmetic.Source1Amount", (float)(1 - (double)e.NewValue));
                brush.CompositionBrush?.Properties.InsertScalar("Arithmetic.Source2Amount", (float)(double)e.NewValue);
            }
            else
            {
                brush.Brush?.Properties.InsertScalar("Arithmetic.Source1Amount", (float)(1 - (double)e.NewValue));
                brush.Brush?.Properties.InsertScalar("Arithmetic.Source2Amount", (float)(double)e.NewValue);
            }
        }

        #endregion

        #region TintTransitionDuration

        /// <summary>
        /// Identifies the <see cref="TintTransitionDuration"/> dependency property.
        /// </summary>
#if !WINRT
        readonly
#endif
        public static DependencyProperty TintTransitionDurationProperty
#if WINRT
            => _tintTransitionDurationProperty;

        private static readonly DependencyProperty _tintTransitionDurationProperty
#endif
            = DependencyProperty.Register(
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
        public BackdropBlurBrush()
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

                if (Window.Current != null)
                {
                    Compositor = Window.Current.Compositor;
                }

                if (Compositor == null) { return; }

                if (!AlwaysUseFallback)
                {
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
#if !NET
                            if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush"))
                            {
                                backdrop = Compositor.TryCreateBlurredWallpaperBackdropBrush();
                            }
                            else
#endif
                            if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush"))
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

                    // Use a Win2D blur affect applied to a CompositionBackdropBrush.
                    ArithmeticCompositeEffect compositeEffect = new ArithmeticCompositeEffect
                    {
                        Name = "Arithmetic",
                        MultiplyAmount = 0f,
                        Source1Amount = (float)(1f - TintOpacity),
                        Source2Amount = (float)TintOpacity,
                        Source1 = new GaussianBlurEffect
                        {
                            Name = "Blur",
                            BlurAmount = (float)Amount,
                            BorderMode = EffectBorderMode.Hard,
                            Optimization = EffectOptimization.Balanced,
                            Source = new CompositionEffectSourceParameter("backdrop"),
                        },
                        Source2 = new ColorSourceEffect
                        {
                            Name = "TintColor",
                            Color = TintColor
                        }
                    };

                    CompositionEffectFactory effectFactory = Compositor.CreateEffectFactory(compositeEffect, new[] { "Blur.BlurAmount", "Arithmetic.Source1Amount", "Arithmetic.Source2Amount", "TintColor.Color" });
                    CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

                    effectBrush.SetSourceParameter("backdrop", backdrop);

                    Brush = effectBrush;
                    CompositionBrush = Brush;

                    LinearEasingFunction line = Compositor.CreateLinearEasingFunction();

                    TimeSpan duration = TintTransitionDuration == TimeSpan.Zero ? TimeSpan.FromTicks(10000) : TintTransitionDuration;
                    TimeSpan switchDuration = TimeSpan.FromMilliseconds(167);

                    TintOpacityFillAnimation = Compositor.CreateScalarKeyFrameAnimation();
                    TintOpacityFillAnimation.InsertExpressionKeyFrame(0f, "TintOpacity", line);
                    TintOpacityFillAnimation.InsertKeyFrame(1f, 1f, line);
                    TintOpacityFillAnimation.Duration = switchDuration;
                    TintOpacityFillAnimation.Target = "Arithmetic.Source2Amount";

                    HostOpacityZeroAnimation = Compositor.CreateScalarKeyFrameAnimation();
                    HostOpacityZeroAnimation.InsertExpressionKeyFrame(0f, "1f - TintOpacity", line);
                    HostOpacityZeroAnimation.InsertKeyFrame(1f, 0f, line);
                    HostOpacityZeroAnimation.Duration = switchDuration;
                    HostOpacityZeroAnimation.Target = "Arithmetic.Source1Amount";

                    TintToFallBackAnimation = Compositor.CreateColorKeyFrameAnimation();
                    TintToFallBackAnimation.InsertExpressionKeyFrame(0f, "TintColor", line);
                    TintToFallBackAnimation.InsertExpressionKeyFrame(1f, "FallbackColor", line);
                    TintToFallBackAnimation.Duration = duration;
                    TintToFallBackAnimation.Target = "TintColor.Color";

                    TintToFallBackAnimation?.SetColorParameter("TintColor", TintColor);
                    HostOpacityZeroAnimation?.SetScalarParameter("TintOpacity", (float)TintOpacity);
                    TintOpacityFillAnimation?.SetScalarParameter("TintOpacity", (float)TintOpacity);

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
                CompositionBrush.StartAnimation("Arithmetic.Source2Amount", TintOpacityFillAnimation);
                CompositionBrush.StartAnimation("Arithmetic.Source1Amount", HostOpacityZeroAnimation);
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
                CompositionBrush.StartAnimation("Arithmetic.Source2Amount", TintOpacityFillAnimation);
                CompositionBrush.StartAnimation("Arithmetic.Source1Amount", HostOpacityZeroAnimation);
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
