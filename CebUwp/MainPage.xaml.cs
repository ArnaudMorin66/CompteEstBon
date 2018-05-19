using CompteEstBon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace CebUwp
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            // this.bindTirage.Main = this;
        }
        private async void Hasard_Click(object sender, RoutedEventArgs e)
        {
            await bindTirage.RandomAsync();
        }

        private async void Resoudre_Click(object sender, RoutedEventArgs e)
        {
            switch (bindTirage.Tirage.Status)
            {
                case CebStatus.Valid:
                    await bindTirage.ResolveAsync();
                    cebNotification.Show(5000);

                    break;
                case CebStatus.CompteEstBon:
                case CebStatus.CompteApproche:
                    await bindTirage.ClearAsync();
                    break;
                case CebStatus.Erreur:
                    await bindTirage.RandomAsync();
                    break;
            }
        }


        private void Result_Click(object sender, RoutedEventArgs e)
        {
            if (bindTirage.Tirage.Status == CebStatus.CompteApproche || bindTirage.Tirage.Status == CebStatus.CompteEstBon)
                cebNotification.Show(5000);
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

        private async void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            await bindTirage.ExportToCsvAsync();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            this.bindTirage.dateDispatcher.Stop();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
                view.ExitFullScreenMode();
            else
                view.TryEnterFullScreenMode();
        }
    }
}
