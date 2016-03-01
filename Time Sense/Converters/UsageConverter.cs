using System;
using Stuff;
using Windows.UI.Xaml.Data;

namespace Time_Sense.Converters
{
    public class UsageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return utilities.FormatData(int.Parse(value.ToString()));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }    
    }
}
