using MicaForUWP.Media;
using System;
using System.Reflection;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace MicaDemo.Helpers.ValueConverters
{
    internal class TypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) { return value; }
            object result = value is BackgroundSource BackgroundSource ? BackgroundSource : value is Color Color ? Color : value;
            return targetType.IsInstanceOfType(result) ? result : XamlBindingHelper.ConvertValue(targetType, result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            targetType.IsInstanceOfType(value) ? value : XamlBindingHelper.ConvertValue(targetType, value);
    }
}
