using MicaForUWP.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace MicaForUWP.Media
{
    /// <summary>
    /// The <see cref="BackdropMicaBrush"/> is a <see cref="Brush"/> that blurs whatever is behind it in the application.
    /// </summary>
    public class BackdropMicaBrush : XamlCompositionBrushBase
    {
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

            brush.CompositionBrush?.Properties.InsertColor("TintColor.Color", (Color)e.NewValue);
            brush.CompositionBrush?.Properties.InsertColor("LuminosityColor.Color", (Color)e.NewValue);
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
        public float TintOpacity
        {
            get => (float)GetValue(TintOpacityProperty);
            set => SetValue(TintOpacityProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="TintOpacity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TintOpacityProperty = DependencyProperty.Register(
            nameof(TintOpacity),
            typeof(float),
            typeof(BackdropMicaBrush),
            new PropertyMetadata(UIHelper.IsDarkTheme() ? 0.8f : 0.5f, OnTintOpacityPropertyChanged));

        /// <summary>
        /// Updates the UI when <see cref="TintOpacity"/> changes
        /// </summary>
        /// <param name="d">The current <see cref="BackdropMicaBrush"/> instance</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="TintOpacityProperty"/></param>
        private static void OnTintOpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackdropMicaBrush brush = (BackdropMicaBrush)d;

            brush.CompositionBrush?.Properties.InsertScalar("TintOpacity.Opacity", (float)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the tint opacity factor for the effect (default is 0.5, must be in the [0, 1] range)
        /// </summary>
        public float LuminosityOpacity
        {
            get => (float)GetValue(LuminosityOpacityProperty);
            set => SetValue(LuminosityOpacityProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="LuminosityOpacity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LuminosityOpacityProperty = DependencyProperty.Register(
            nameof(LuminosityOpacity),
            typeof(float),
            typeof(BackdropMicaBrush),
            new PropertyMetadata(1f, OnLuminosityOpacityPropertyChanged));

        /// <summary>
        /// Updates the UI when <see cref="LuminosityOpacity"/> changes
        /// </summary>
        /// <param name="d">The current <see cref="BackdropMicaBrush"/> instance</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="TintOpacityProperty"/></param>
        private static void OnLuminosityOpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackdropMicaBrush brush = (BackdropMicaBrush)d;

            brush.CompositionBrush?.Properties.InsertScalar("LuminosityOpacity.Opacity", (float)e.NewValue);
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
                    Opacity = TintOpacity,
                    Source = tintColorEffect
                };

                ColorSourceEffect luminosityColorEffect = new ColorSourceEffect()
                {
                    Color = TintColor,
                    Name = "LuminosityColor"
                };

                OpacityEffect luminosityOpacityEffect = new OpacityEffect()
                {
                    Name = "LuminosityOpacity",
                    Opacity = LuminosityOpacity,
                    Source = luminosityColorEffect
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
                        backdrop = Window.Current.Compositor.CreateBackdropBrush();
                        break;
                    case BackgroundSource.HostBackdrop:
                        backdrop = Window.Current.Compositor.CreateHostBackdropBrush();
                        break;
                    case BackgroundSource.MicaBackdrop:
                        backdrop = Window.Current.Compositor.TryCreateBlurredWallpaperBackdropBrush();
                        break;
                    default:
                        backdrop = Window.Current.Compositor.TryCreateBlurredWallpaperBackdropBrush();
                        break;
                }

                CompositionEffectBrush micaEffectBrush = Window.Current.Compositor.CreateEffectFactory(colorBlendEffect, new[] { "TintColor.Color", "TintOpacity.Opacity", "LuminosityColor.Color", "LuminosityOpacity.Opacity" }).CreateBrush();
                micaEffectBrush.SetSourceParameter("BlurredWallpaperBackdrop", backdrop);

                CompositionBrush = micaEffectBrush;
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
        }
    }
}
