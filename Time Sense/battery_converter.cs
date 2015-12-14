using System;
using Windows.UI.Xaml.Data;

namespace Time_Sense
{
    public class battery_converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int battery = int.Parse(value.ToString());
            return String.Format("{0}%", battery.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
