using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;

namespace CompteEstBon {

    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private string _style = "MaterialDark";

        public MainWindow() {
            InitializeComponent();
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr");
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            VisualStyle = "MaterialDark";
        }

        public string DotnetVersion => $"{RuntimeInformation.FrameworkDescription}:v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
        public string VisualStyle {
            get => _style;
            set {
                var resource = Resources.MergedDictionaries.First(v => v.Source.OriginalString.Contains(_style));
                _style = value;
                
                SfSkinManager.SetVisualStyle(this, (VisualStyles) Enum.Parse(typeof(VisualStyles), value));
                resource.Source =
                    new Uri($"pack://application:,,,/Syncfusion.Themes.{value}.WPF;component/Common/Brushes.xaml");
                // SkinStorage.SetVisualStyle(this, value);
            }
        }

        private void SolutionsData_SelectionChanged(object sender, GridSelectionChangedEventArgs e) => ViewTirage.ShowPopup(SolutionsData.SelectedIndex);

        private void SolutionsData_QueryRowHeight(object sender, QueryRowHeightEventArgs e) {
            if (e.RowIndex <= 0) return;
            var gridRowResizingOptions = new GridRowSizingOptions();
            if (!SolutionsData.GridColumnSizer.GetAutoRowHeight(e.RowIndex, gridRowResizingOptions,
                out var autoHeight)) return;
            e.Height = autoHeight;
            e.Handled = true;
        }


        private void btnMode_Click(object sender, RoutedEventArgs e) {
            ViewTirage.Vertical = !ViewTirage.Vertical;
            if (SolutionsData.GetRecordsCount() <= 0) return;
            for (var index = 0; index <= SolutionsData.GetRecordsCount(); index++)
                SolutionsData.InvalidateRowHeight(index);
        }

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (sender is UIElement element) {
                e.Handled = true;

                var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {
                    RoutedEvent = MouseWheelEvent
                };
                element.RaiseEvent(e2);
            }
        }
    }
}