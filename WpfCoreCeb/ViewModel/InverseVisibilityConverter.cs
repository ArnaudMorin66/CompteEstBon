using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace CompteEstBon.ViewModel {
    [ValueConversion(typeof(Visibility), typeof(Visibility))]
    class InverseVisibilityConverter: IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Visibility v ? v== Visibility.Visible  ? Visibility.Hidden : Visibility.Visible : (object)Visibility.Hidden;

        

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

      
    }
}
