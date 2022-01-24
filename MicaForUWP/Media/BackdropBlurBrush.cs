using Microsoft.Graphics.Canvas.Effects;
using System;
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
    public class BackdropBlurBrush : XamlCompositionBrushBase
    {
        CompositionEffectBrush Brush;
        ScalarKeyFrameAnimation TintOpacityFillAnimation;
        ScalarKeyFrameAnimation HostOpacityZeroAnimation;
        ColorKeyFrameAnimation TintToFallBackAnimation;

        /// <summary>
        /// Identifies the <see cref="Amount"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AmountProperty = DependencyProperty.Register(
            nameof(Amount),
            typeof(double),
            typeof(BackdropBlurBrush),
            new PropertyMetadata(0.0d, new PropertyChangedCallback(OnAmountChanged)));

        /// <summary>
        /// Gets or sets the amount of gaussian blur to apply to the background.
        /// </summary>
        public double Amount
        {
            get { return (double)GetValue(AmountProperty); }
            set { SetValue(AmountProperty, value); }
        }

        private static void OnAmountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackdropBlurBrush brush = (BackdropBlurBrush)d;
            if ((double)e.NewValue > 100) { brush.Amount = 100d; }
            else if ((double)e.NewValue < 0) { brush.Amount = 0d; }
            // Unbox and set a new blur amount if the CompositionBrush exists.
            brush.CompositionBrush?.Properties.InsertScalar("Blur.BlurAmount", (float)(double)e.NewValue);
        }

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
            typeof(BackdropBlurBrush),
            new PropertyMetadata(default(Color), OnTintColorPropertyChanged));

        /// <summary>
        /// Updates the UI when <see cref="TintColor"/> changes
        /// </summary>
        /// <param name="d">The current <see cref="AcrylicBrush"/> instance</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="TintColorProperty"/></param>
        private static void OnTintColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackdropBlurBrush brush = (BackdropBlurBrush)d;

            brush.TintToFallBackAnimation?.SetColorParameter("TintColor", (Color)e.NewValue);
            brush.CompositionBrush?.Properties.InsertColor("TintColor.Color", (Color)e.NewValue);
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
            typeof(BackdropBlurBrush),
            new PropertyMetadata(BackgroundSource.Backdrop, OnSourcePropertyChanged));

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
            typeof(BackdropBlurBrush),
            new PropertyMetadata(0.5d, OnTintOpacityPropertyChanged));

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
            brush.CompositionBrush?.Properties.InsertScalar("Arithmetic.Source1Amount", (float)(1 - (double)e.NewValue));
            brush.CompositionBrush?.Properties.InsertScalar("Arithmetic.Source2Amount", (float)(double)e.NewValue);
        }

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

                CompositionBackdropBrush backdrop;

                switch (BackgroundSource)
                {
                    case BackgroundSource.Backdrop:
                        backdrop = Window.Current.Compositor.CreateBackdropBrush();
                        break;
                    case BackgroundSource.HostBackdrop:
                        backdrop = Window.Current.Compositor.CreateHostBackdropBrush();
                        break;
                    case BackgroundSource.MicaBackdrop:
                        backdrop = Window.Current.Compositor.TryCreateBlurredWallpaperBackdropBrush();
                        break;
                    default:
                        backdrop = Window.Current.Compositor.CreateBackdropBrush();
                        break;
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

                CompositionEffectFactory effectFactory = Window.Current.Compositor.CreateEffectFactory(compositeEffect, new[] { "Blur.BlurAmount", "Arithmetic.Source1Amount", "Arithmetic.Source2Amount", "TintColor.Color" });
                CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

                effectBrush.SetSourceParameter("backdrop", backdrop);

                Brush = effectBrush;
                CompositionBrush = Brush;

                LinearEasingFunction line = Window.Current.Compositor.CreateLinearEasingFunction();

                TintOpacityFillAnimation = Window.Current.Compositor.CreateScalarKeyFrameAnimation();
                TintOpacityFillAnimation.InsertExpressionKeyFrame(0f, "TintOpacity", line);
                TintOpacityFillAnimation.InsertKeyFrame(1f, 1f, line);
                TintOpacityFillAnimation.Duration = TimeSpan.FromSeconds(0.1d);
                TintOpacityFillAnimation.Target = "Arithmetic.Source2Amount";

                HostOpacityZeroAnimation = Window.Current.Compositor.CreateScalarKeyFrameAnimation();
                HostOpacityZeroAnimation.InsertExpressionKeyFrame(0f, "1f - TintOpacity", line);
                HostOpacityZeroAnimation.InsertKeyFrame(1f, 0f, line);
                HostOpacityZeroAnimation.Duration = TimeSpan.FromSeconds(0.1d);
                HostOpacityZeroAnimation.Target = "Arithmetic.Source1Amount";

                TintToFallBackAnimation = Window.Current.Compositor.CreateColorKeyFrameAnimation();
                TintToFallBackAnimation.InsertExpressionKeyFrame(0f, "TintColor", line);
                TintToFallBackAnimation.InsertExpressionKeyFrame(1f, "FallbackColor", line);
                TintToFallBackAnimation.Duration = TimeSpan.FromSeconds(0.1d);
                TintToFallBackAnimation.Target = "TintColor.Color";

                TintToFallBackAnimation?.SetColorParameter("TintColor", TintColor);
                HostOpacityZeroAnimation?.SetScalarParameter("TintOpacity", (float)TintOpacity);
                TintOpacityFillAnimation?.SetScalarParameter("TintOpacity", (float)TintOpacity);

                CoreWindow.GetForCurrentThread().Activated += CoreWindow_Activated;
                CoreWindow.GetForCurrentThread().VisibilityChanged += CoreWindow_VisibilityChanged;
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

        private void SetCompositionFocus(bool IsGotFocus)
        {
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
            else if(CompositionBrush == Brush)
            {
                CompositionScopedBatch scopedBatch = Window.Current.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                TintOpacityFillAnimation.Direction = AnimationDirection.Normal;
                HostOpacityZeroAnimation.Direction = AnimationDirection.Normal;
                TintToFallBackAnimation.Direction = AnimationDirection.Normal;
                CompositionBrush.StartAnimation("Arithmetic.Source2Amount", TintOpacityFillAnimation);
                CompositionBrush.StartAnimation("Arithmetic.Source1Amount", HostOpacityZeroAnimation);
                CompositionBrush.StartAnimation("TintColor.Color", TintToFallBackAnimation);
                scopedBatch.Completed += (s, a) => CompositionBrush = Window.Current.Compositor.CreateColorBrush(FallbackColor);
                scopedBatch.End();
            }
            else
            {
                CompositionBrush = Window.Current.Compositor.CreateColorBrush(FallbackColor);
            }
        }
    }

    public enum BackgroundSource
    {
        //
        // 摘要:
        //     画笔从应用窗口后面的内容采样。
        HostBackdrop = 0,
        //
        // 摘要:
        //     画笔从应用窗口后面的壁纸采样。
        MicaBackdrop = 1,
        //
        // 摘要:
        //     画笔从应用内容采样。
        Backdrop = 2
    }
}
