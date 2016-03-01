using System;
using Stuff;
using Windows.UI.Xaml.Data;

namespace Time_Sense.Converters
{
    class UnlocksConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string unlocks = value == null ? "0" : value.ToString();
            return String.Format(utilities.loader.GetString("unlocks"), unlocks);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
