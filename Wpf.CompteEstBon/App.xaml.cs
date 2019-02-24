using System.Windows;
using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;

namespace CompteEstBon {
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application {
        public App() {
            SyncfusionLicenseProvider.RegisterLicense(CompteEstBon.Properties.Settings.Default.Licence);
            SfSkinManager.ApplyStylesOnApplication = true;
        }
    }
}
