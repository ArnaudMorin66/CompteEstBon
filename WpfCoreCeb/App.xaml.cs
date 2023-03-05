using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace CompteEstBon;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App {
    private static string FindLicenseKey() => CompteEstBon.Properties.Resources.Syncfusion;

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