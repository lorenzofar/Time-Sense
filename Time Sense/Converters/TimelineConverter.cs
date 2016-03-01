using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Time_Sense.Converters
{
    public class TimelineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string state = value as string;
            return state == "on" || state == "charging" ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
