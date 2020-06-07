using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CompteEstBon {
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ChromelessWindow {
        public MainWindow() {
            InitializeComponent();
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr"); 
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
        private void SolutionsData_SelectionChanged(object sender, GridSelectionChangedEventArgs e) {
            View.ShowPopup(SolutionsData.SelectedIndex);
        }
        private ViewTirage View => FindResource("Tirage") as ViewTirage;

        private void SolutionsData_QueryRowHeight(object sender, QueryRowHeightEventArgs e) {
            if (e.RowIndex > 0) {
                var gridRowResizingOptions = new GridRowSizingOptions();
                if (SolutionsData.GridColumnSizer.GetAutoRowHeight(e.RowIndex, gridRowResizingOptions, out double autoHeight)) {
                    e.Height = autoHeight;
                    e.Handled = true;

                }

            }
        }

        private void btnMode_Click(object sender, RoutedEventArgs e) {
            View.Vertical = !View.Vertical;
            if (SolutionsData.GetRecordsCount() > 0) {
                for (var index = 0; index <= SolutionsData.GetRecordsCount(); index++) {
                    SolutionsData.InvalidateRowHeight(index);
                }
            }

        }

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            UIElement element = sender as UIElement;

            if (element != null) {
                e.Handled = true;

                var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {
                    RoutedEvent = UIElement.MouseWheelEvent
                };
                element.RaiseEvent(e2);
            }
        }
    }
}
