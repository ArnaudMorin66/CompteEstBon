using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CompteEstBon {
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame. 
    /// </summary>
    public sealed partial class MainPage : Page {
        public MainPage() {
            InitializeComponent();
            bindTirage.storyBoard = TextBlockBoard;
            TextBlockBoard.Begin();
            TextBlockBoard.Pause();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e) {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode) {
                view.ExitFullScreenMode();
            }
            else {
                view.TryEnterFullScreenMode();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e) => bindTirage.dateDispatcher.Stop();




        private void SolutionsData_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            bindTirage.ShowNotify(SolutionsData.SelectedIndex);

        }

        private void TbMoins_Click(object sender, RoutedEventArgs e) {
            if (bindTirage.Search > 100) {
                bindTirage.Search--;
            }
        }

        private void TbPlus_Click(object sender, RoutedEventArgs e) {
            if (bindTirage.Search < 999) {
                bindTirage.Search++;
            }
        }

        private void SelectSolution(object sender, RoutedEventArgs e) {
            bindTirage.ShowNotify(SolutionsData.SelectedIndex);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {

        }

    }
}