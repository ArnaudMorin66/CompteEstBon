using CompteEstBon;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CebUwp
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame. 
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage() => InitializeComponent();

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
            }
            else
            {
                view.TryEnterFullScreenMode();
            }
        }

        private async void ExportCSV_Click(object sender, RoutedEventArgs e) => await bindTirage.ExportToCsvAsync();

        private async void Hasard_Click(object sender, RoutedEventArgs e)
        {
            await bindTirage.RandomAsync();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e) => this.bindTirage.dateDispatcher.Stop();

        private async void Resoudre_Click(object sender, RoutedEventArgs e)
        {
            switch (bindTirage.Tirage.Status)
            {
            case CebStatus.Valid:
                await bindTirage.ResolveAsync();
                SolutionsData.SelectedIndex = 0;
                break;

            case CebStatus.CompteEstBon:
            case CebStatus.CompteApproche:
                SolutionsData.SelectedIndex = -1;
                await bindTirage.ClearAsync();
                break;

            case CebStatus.Erreur:
                SolutionsData.SelectedIndex = -1;
                await bindTirage.RandomAsync();
                break;
            }
        }

        private void Result_Click(object sender, RoutedEventArgs e)
        {
            cebNotification.Show(5000);
        }

        private void SolutionsData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SolutionsData.SelectedIndex == -1)
            {
                cebNotification.Dismiss();
                return;
            }
            bindTirage.CurrentSolution = bindTirage.SolutionToString(SolutionsData.SelectedIndex);
            cebNotification.Show(10000);
        }

        private void TbMoins_Click(object sender, RoutedEventArgs e)
        {
            if (bindTirage.Search > 100)
            {
                bindTirage.Search--;
            }
        }

        private void TbPlus_Click(object sender, RoutedEventArgs e)
        {
            if (bindTirage.Search < 999)
            {
                bindTirage.Search++;
            }
        }
    }
}