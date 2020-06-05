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
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            SfSkinManager.SetVisualStyle(this, VisualStyles.Default);
        }
        private void SolutionsData_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e) {
            View.ShowPopup(SolutionsData.SelectedIndex);
        }
        private ViewTirage View => FindResource("Tirage") as ViewTirage;

        private void SolutionsData_QueryRowHeight(object sender, Syncfusion.UI.Xaml.Grid.QueryRowHeightEventArgs e) {
            if (e.RowIndex > 0) {
                GridRowSizingOptions gridRowResizingOptions = new GridRowSizingOptions();
                if (SolutionsData.GridColumnSizer.GetAutoRowHeight(e.RowIndex, gridRowResizingOptions, out double autoHeight)) {
                        e.Height = autoHeight;
                        e.Handled = true;
          
                }
                
            }
        }

        private void btnMode_Click(object sender, RoutedEventArgs e) {
            View.Vertical = !View.Vertical;
            if (SolutionsData.GetRecordsCount() > 0) {
                
                for (var index = 0; index < SolutionsData.GetRecordsCount(); index++) {
                
                    SolutionsData.InvalidateRowHeight(index);

                }

            }

        }
    }
}
