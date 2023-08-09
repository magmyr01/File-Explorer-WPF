using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace File_Explorer.Converter
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class ItemSetToMenuItemVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(Clipboard.ContainsText()) { return TrueValue; }
            else { return FalseValue; }

            if(value is null) { return FalseValue; }
            else { return TrueValue; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
