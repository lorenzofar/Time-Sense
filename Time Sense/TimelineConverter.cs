using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Time_Sense
{
    public class TimelineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string state = value as string;
            if(state == "on" || state == "charging")
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
