using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace CompteEstBon {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public App() {
            SyncfusionLicenseProvider.RegisterLicense(CompteEstBon.Properties.Settings.Default.SfLicense);
            SfSkinManager.ApplyStylesOnApplication = true;
        }
    }
}
