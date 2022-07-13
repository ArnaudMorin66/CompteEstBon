using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;
using System;
using System.Linq;
using System.Windows;


namespace CompteEstBon {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public App() {
            SyncfusionLicenseProvider.RegisterLicense(CompteEstBon.Properties.Settings.Default.SfLicence);
            SfCebOffice.RegisterLicense(CompteEstBon.Properties.Settings.Default.SfLicence);
            SfSkinManager.ApplyStylesOnApplication = true;
        }
    }

}
