﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CompteEstBon.ViewModel {
    [ValueConversion(typeof(bool), typeof(Visibility))]
    class BusyVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b ? b ? Visibility.Visible : Visibility.Collapsed : (object)Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}