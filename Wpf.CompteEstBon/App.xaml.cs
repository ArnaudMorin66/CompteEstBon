using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;
using System.Windows;

namespace CompteEstBon {
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application {
        public App() {
            SyncfusionLicenseProvider.RegisterLicense(CompteEstBon.Properties.Resources.License);
            SfSkinManager.ApplyStylesOnApplication = true;
        }
    }
}
