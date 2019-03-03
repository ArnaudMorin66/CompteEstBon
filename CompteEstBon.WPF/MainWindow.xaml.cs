using System.Windows;

namespace CompteEstBon
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow:  Window { 
        public MainWindow() {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void SolutionsData_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Tirage.ShowNotify(SolutionsData.SelectedIndex);
        }

        private void TbPlus_Click(object sender, RoutedEventArgs e)
        {
            if (Tirage.Search < 999)
            {
                Tirage.Search++;
            }
        }

        private void TbMoins_Click(object sender, RoutedEventArgs e)
        {
            if (Tirage.Search > 100)
            {
                Tirage.Search--;
            }
        }

     
    }
}
