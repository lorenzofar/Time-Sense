using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Time_Sense.Converters
{
    class NoteEditConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool editing = bool.Parse(value.ToString());
            return editing ? new SymbolIcon(Symbol.Save) : new SymbolIcon(Symbol.Edit);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
