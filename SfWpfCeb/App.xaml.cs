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
            SyncfusionLicenseProvider.RegisterLicense(FindLicenseKey());
            SfSkinManager.ApplyStylesOnApplication = true;
        }
        public static string FindLicenseKey() {
            int num = 12;
            string path = "SyncfusionLicense.txt";
            string text = Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase.Replace("file:///", ""));
            for (int i = 0; i < num; i++) {
                string path2 = Path.Combine(text, path);
                if (File.Exists(path2)) {
                    return File.ReadAllText(path2, Encoding.UTF8);
                }
                var parent = Directory.GetParent(text);
                if (parent == null) {
                    break;
                }
                text = parent.FullName;
            }
            return string.Empty;
        }
    }
    
}
