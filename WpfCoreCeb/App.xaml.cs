using Syncfusion.Licensing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace CompteEstBon {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App: Application {
        public App() {
            

        }
        
        protected override void OnStartup(StartupEventArgs e) {
            var vCulture = new CultureInfo("fr-FR");

            Thread.CurrentThread.CurrentCulture = vCulture;
            Thread.CurrentThread.CurrentUICulture = vCulture;
            CultureInfo.DefaultThreadCurrentCulture = vCulture;
            CultureInfo.DefaultThreadCurrentUICulture = vCulture;
            
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement), new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            SfCebOffice.RegisterLicense(FindLicenseKey());
            base.OnStartup(e);
        }
        private static string FindLicenseKey() {
            
            string path = "SyncfusionLicense.txt";
            string text = Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase.Replace("file:///", ""));
            while (!string.IsNullOrEmpty(text)) {  
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
