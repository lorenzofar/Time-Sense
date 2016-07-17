using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Time_Sense.Converters
{
    public class IndexVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int pivotIndex = int.Parse(value.ToString());
            int elementIndex = int.Parse(parameter.ToString());
            return pivotIndex == elementIndex ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
