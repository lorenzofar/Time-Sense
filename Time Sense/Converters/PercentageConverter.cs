using System;
using Windows.UI.Xaml.Data;

namespace Time_Sense.Converters
{
    class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double usage = value == null ? 0 : double.Parse(value.ToString());
            double percentage = Math.Round((usage / 86400) * 100, 2);
            return $"{percentage}%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
