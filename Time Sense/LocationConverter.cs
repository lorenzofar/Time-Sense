using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Time_Sense
{
    public class LocationConverter : DependencyObject, IValueConverter
    {         
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double latitude = double.Parse(value.ToString());
            return latitude != 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
