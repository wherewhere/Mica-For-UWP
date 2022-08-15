using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace MicaDemo.Controls
{
    public sealed partial class ColorPickerEx : UserControl
    {
        private Slider ASlider;
        private Slider RSlider;
        private Slider GSlider;
        private Slider BSlider;
        private ColorPicker ColorPicker;

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
           nameof(Color),
           typeof(Color),
           typeof(ColorPickerEx),
           new PropertyMetadata(default(Color), OnColorPropertyChanged));

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!ApiInformation.IsTypePresent("Windows.UI.Xaml.Controls.ColorPicker"))
            {
                (d as ColorPickerEx).OnColorPropertyChanged(e);
            }
        }

        public ColorPickerEx()
        {
            InitializeComponent();
            CreateColorPicker();
        }

        private void CreateColorPicker()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Controls.ColorPicker"))
            {
                ColorPicker = new ColorPicker
                {
                    IsAlphaEnabled = true,
                    IsAlphaSliderVisible = true,
                    IsAlphaTextInputVisible = true,
                    IsMoreButtonVisible = true,
                };
                ColorPicker.SetBinding(ColorPicker.ColorProperty, new Binding
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                    Path = new PropertyPath(nameof(Color))
                });
                Content = ColorPicker;
            }
            else
            {
                StackPanel StackPanel = new StackPanel();

                ASlider = new Slider
                {
                    Header = "Alpha",
                    Minimum = 0,
                    Maximum = 255,
                    StepFrequency = 1
                };
                RSlider = new Slider
                {
                    Header = "Red",
                    Minimum = 0,
                    Maximum = 255,
                    StepFrequency = 1
                };
                GSlider = new Slider
                {
                    Header = "Green",
                    Minimum = 0,
                    Maximum = 255,
                    StepFrequency = 1
                };
                BSlider = new Slider
                {
                    Header = "Blue",
                    Minimum = 0,
                    Maximum = 255,
                    StepFrequency = 1
                };

                ASlider.ValueChanged += OnValueChanged;
                RSlider.ValueChanged += OnValueChanged;
                GSlider.ValueChanged += OnValueChanged;
                BSlider.ValueChanged += OnValueChanged;

                StackPanel.Children.Add(ASlider);
                StackPanel.Children.Add(RSlider);
                StackPanel.Children.Add(GSlider);
                StackPanel.Children.Add(BSlider);

                Content = StackPanel;
            }
        }

        private void SetSliderValue(Color Color)
        {
            if (ASlider != null) { ASlider.Value = Color.A; }
            if (RSlider != null) { RSlider.Value = Color.R; }
            if (GSlider != null) { GSlider.Value = Color.G; }
            if (BSlider != null) { BSlider.Value = Color.B; }
        }

        private void OnColorPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue && e.NewValue is Color Color)
            {
                SetSliderValue(Color);
            }
        }

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                Color = Color.FromArgb((byte)ASlider.Value, (byte)RSlider.Value, (byte)GSlider.Value, (byte)BSlider.Value);
            }
        }
    }
}
