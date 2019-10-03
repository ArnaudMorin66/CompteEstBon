using System;
using Windows.UI.Xaml.Data;

namespace CompteEstBon {
    public class FormatConverter : IValueConverter {
        // This converts the DateTime object to the string to display.
        //
        // Résumé :
        //     /// Convert an System.IFormattable value to a formatted System.String. ///
        //
        // Paramètres :
        //   value:
        //     The source data being passed to the target.
        //
        //   targetType:
        //     The type of the target property, as a type reference.
        //
        //   parameter:
        //     The format string.
        //
        //   language:
        //     The language of the conversion. Not used.
        //
        // Retourne :
        //     The formatted string.
        public object Convert(object value, Type targetType, object parameter, string language) {
            var formattable = value as IFormattable;
            var text = parameter as string;
            if (formattable == null || text == null) {
                return value;
            }
            return formattable.ToString(text, null);
        }

        //
        // Résumé :
        //     /// Not implemented. ///
        //
        // Paramètres :
        //   value:
        //     The source data being passed to the target.
        //
        //   targetType:
        //     The type of the target property, as a type reference.
        //
        //   parameter:
        //     Optional parameter. Not used.
        //
        //   language:
        //     The language of the conversion. Not used.
        //
        // Retourne :
        //     The value to be passed to the target dependency property.
        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}

