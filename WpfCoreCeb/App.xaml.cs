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
    public partial class App : Application {
        public App() { }

        private static string FindLicenseKey() {
            var path = "SyncfusionLicense.txt";
            var text = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            while(!string.IsNullOrEmpty(text)) {
                var path2 = Path.Combine(text, path);
                if(File.Exists(path2)) {
                    return File.ReadAllText(path2, Encoding.UTF8);
                }
                var parent = Directory.GetParent(text);
                if(parent == null) {
                    break;
                }
                text = parent.FullName;
            }
            return string.Empty;
        }

        protected override void OnStartup(StartupEventArgs e) {
            CultureInfo vCulture = new("fr-FR");

            Thread.CurrentThread.CurrentCulture = vCulture;
            Thread.CurrentThread.CurrentUICulture = vCulture;
            CultureInfo.DefaultThreadCurrentCulture = vCulture;
            CultureInfo.DefaultThreadCurrentUICulture = vCulture;

            FrameworkElement.LanguageProperty
                .OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            ExportOffice.RegisterLicense(FindLicenseKey());
            base.OnStartup(e);
        }
    }
}
