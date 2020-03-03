using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CompteEstBon {
    public static class IgnoreScrollBehaviour {
        public static readonly DependencyProperty IgnoreScrollProperty = DependencyProperty.RegisterAttached("IgnoreScroll", typeof(bool), typeof(IgnoreScrollBehaviour), new PropertyMetadata(OnIgnoreScollChanged));

        public static void SetIgnoreScroll(DependencyObject o, string value) {
            o.SetValue(IgnoreScrollProperty, value);
        }

        public static string GetIgnoreScroll(DependencyObject o) {
            return (string)o.GetValue(IgnoreScrollProperty);
        }

        private static void OnIgnoreScollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            bool ignoreScoll = (bool)e.NewValue;
            UIElement element = d as UIElement;

            if (element == null)
                return;

            if (ignoreScoll) {
                element.PreviewMouseWheel += Element_PreviewMouseWheel;
            }
            else {
                element.PreviewMouseWheel -= Element_PreviewMouseWheel;
            }
        }

        private static void Element_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            UIElement element = sender as UIElement;

            if (element != null) {
                e.Handled = true;

                var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                e2.RoutedEvent = UIElement.MouseWheelEvent;
                element.RaiseEvent(e2);
            }
        }
    }
}
