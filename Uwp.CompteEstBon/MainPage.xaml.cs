using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CompteEstBon {
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        public MainPage() {
            InitializeComponent();
        }

        private void AppFullscreen(object sender, RoutedEventArgs e) {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode) {
                view.ExitFullScreenMode();
            }
            else {
                view.TryEnterFullScreenMode();
            }
        }
    }
}
