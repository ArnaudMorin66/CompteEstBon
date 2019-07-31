using System;
using Windows.UI.Xaml.Data;

namespace CompteEstBon.ViewModel {
    public class InverseBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value is bool b) {
                return !b;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
