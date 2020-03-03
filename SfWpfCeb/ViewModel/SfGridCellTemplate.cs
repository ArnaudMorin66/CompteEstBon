using Syncfusion.UI.Xaml.Grid.Cells;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CompteEstBon {
    class SfGridCellTemplateSelector: DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
           
                return Application.Current.Resources["verticalbox"] as DataTemplate;

  
        }
    }
}
