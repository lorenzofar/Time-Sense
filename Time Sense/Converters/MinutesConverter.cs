using System;
using Database;
using Windows.UI.Xaml.Data;
using System.Collections.Generic;

namespace Time_Sense.Converters
{
    class MinutesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                List<Hour> list = value as List<Hour>;
                foreach (var item in list)
                {
                    item.usage = item.usage / 60;
                }
                return list;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
