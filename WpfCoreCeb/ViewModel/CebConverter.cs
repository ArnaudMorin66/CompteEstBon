﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CompteEstBon.ViewModel {
    // [ValueConversion(typeof(bool), typeof(Visibility))]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    internal class CebConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var inverse = parameter is bool bl && bl;
            return targetType.Name switch {
                // Retour Visibility
                nameof(Visibility) => value switch {
                    bool vb => (inverse ? !vb : vb) ? Visibility.Visible : Visibility.Hidden,
                    Visibility visibility => ( inverse && visibility != Visibility.Visible) ||
                                             (!inverse && visibility == Visibility.Visible) ? Visibility.Visible : Visibility.Hidden,
                    CebStatus status => ( inverse && !(status == CebStatus.CompteApproche || status == CebStatus.CompteEstBon)) ||
                                        (!inverse &&  (status == CebStatus.CompteApproche || status == CebStatus.CompteEstBon))
                            ? Visibility.Visible : Visibility.Hidden, 
                       
                    _ => Visibility.Hidden
                },
                // Retour boolean
                nameof(Boolean) => value switch {
                    bool vb => inverse ? !vb : vb,
                    Visibility visibility => ( inverse && visibility != Visibility.Visible) ||
                                             (!inverse && visibility == Visibility.Visible),
                    CebStatus status => ( inverse && !(status == CebStatus.CompteEstBon || status == CebStatus.CompteApproche)) || 
                                        (!inverse &&  (status == CebStatus.CompteEstBon || status == CebStatus.CompteApproche)),
                    _ => false
                },
                _ => throw new Exception(targetType.Name)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        private string GetDebuggerDisplay() {
            return ToString();
        }
    }
}