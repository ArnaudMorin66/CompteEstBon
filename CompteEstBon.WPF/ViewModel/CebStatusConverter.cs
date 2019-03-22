#region

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace CompteEstBon.ViewModel {
    public class CebStatusConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            // ReSharper disable once PossibleNullReferenceException
            var st =  value as CebStatus?;
            if (targetType == typeof(bool)) {
                return st == CebStatus.CompteApproche || st == CebStatus.CompteEstBon;
            }

            return st == CebStatus.CompteApproche || st == CebStatus.CompteEstBon
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}