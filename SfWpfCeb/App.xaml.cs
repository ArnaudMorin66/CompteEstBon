using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
