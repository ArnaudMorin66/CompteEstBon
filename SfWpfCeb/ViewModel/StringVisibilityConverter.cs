using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CompteEstBon {
    [ValueConversion(typeof(string), typeof(Visibility))]
    class StringVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value == null ? 0 : (object)(value.ToString() == "" ? "0" : "auto");

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
