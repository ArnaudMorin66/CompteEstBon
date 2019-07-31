using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using System.Windows;

namespace CompteEstBon {
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ChromelessWindow {
        public MainWindow() {
            InitializeComponent();

            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            SfSkinManager.SetVisualStyle(this, VisualStyles.Blend);
        }

        private void SolutionsData_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e) {
            Tirage.ShowPopup(SolutionsData.SelectedIndex);
        }
    }
}
