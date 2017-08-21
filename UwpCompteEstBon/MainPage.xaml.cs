
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UwpCompteEstBon
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool _isCtrlKeyPressed;
        public MainPage()
        {
            this.InitializeComponent();
        }
        private async void Hasard_Click(object sender, RoutedEventArgs e)
        {
            await Tirage.RandomAsync();
        }

        private async void Resoudre_Click(object sender, RoutedEventArgs e)
        {
            if (Tirage.Tirage.Status == CompteEstBon.CebStatus.Valid)
            {
                await Tirage.ResolveAsync();
            } else
            {
                await Tirage.ClearAsync();
            }
        }

        private void Grid_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Control)
            {
                _isCtrlKeyPressed = true;
                return;
            }
            if (!_isCtrlKeyPressed) return;
            switch (e.Key)
            {
                case VirtualKey.H:
                    Hasard_Click(sender, e);
                    break;
                case VirtualKey.R:
                    Resoudre_Click(sender, e);
                    break;
            }
        }

        private void Grid_OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Control)
                _isCtrlKeyPressed = false;
        }
    }
}
