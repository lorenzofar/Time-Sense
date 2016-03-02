using System;
using Stuff;
using Windows.UI.Xaml.Data;

namespace Time_Sense.Converters
{
    class UnlocksConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int unlocks = value == null ? 0 : int.Parse(value.ToString());
            return String.Format(utilities.loader.GetString(unlocks == 1 ? "unlock" : "unlocks"), unlocks);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
