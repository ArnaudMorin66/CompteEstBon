using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CebMaui.Helpers {
    public class Int32Converter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is int val) {
                return val.ToString();

            }

            return "";

        }


        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            var b = int.TryParse(value?.ToString(), out var result);
            return b ? result : 0;
        }
    }
}
