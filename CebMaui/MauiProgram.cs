using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;

using Syncfusion.Maui.Core.Hosting;

namespace CebMaui;

public static class MauiProgram {

    public static MauiApp CreateMauiApp() {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("CebMaui.Resources.appsettings.json");

        var config = new ConfigurationBuilder()
            .AddJsonStream(stream!)
            .Build();

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(config["sflicense"]);
        


        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}