using CompteEstBon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
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
            this.Tirage.Main = this;
        }
        private async void Hasard_Click(object sender, RoutedEventArgs e)
        {
            await Tirage.RandomAsync();
        }

        private async void Resoudre_Click(object sender, RoutedEventArgs e)
        {
            switch (Tirage.Tirage.Status)
            {
                case CebStatus.Valid:
                    await Tirage.ResolveAsync();
                    break;
                case CebStatus.CompteEstBon:
                case CebStatus.CompteApproche:
                    await Tirage.ClearAsync();
                    break;
                case CebStatus.Erreur:
                    await Tirage.RandomAsync();
                    break;
            }
        }
    }
}
