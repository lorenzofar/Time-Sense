using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Time_Sense.Converters
{
    class BoolVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool condition = bool.Parse(value.ToString());
            condition = parameter == null ? condition : !condition;
            return condition ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
