using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace CompteEstBon.ViewModel {
    public sealed class BooleanToVisibilityConverterParam: IValueConverter {
       
        /// <summary>
        /// Convert bool or Nullable&lt;bool&gt; to Visibility
        /// </summary>
        /// <param name="value">bool or Nullable&lt;bool&gt;</param>
        /// <param name="targetType">Visibility</param>
        /// <param name="parameter">null</param>
        /// <param name="culture">null</param>
        /// <returns>Visible or Collapsed</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var inversion = parameter is bool and true;
            var bValue = value is bool and true;
            if (inversion) bValue = !bValue;
            return (bValue) ?  Visibility.Hidden: Visibility.Visible;
        }

        /// <summary>
        /// Convert Visibility to boolean
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
