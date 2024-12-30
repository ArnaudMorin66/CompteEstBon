using System.Reflection;

using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Syncfusion.Maui.Core.Hosting;

namespace CebToolkit;

/// <summary>
/// Provides methods to create and configure the .NET MAUI application.
/// </summary>
public static class MauiProgram {
    /// <summary>
    /// Creates and configures the .NET MAUI application.
    /// </summary>
    /// <returns>The configured <see cref="MauiApp"/> instance.</returns>
    public static MauiApp CreateMauiApp() {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("CebToolkit.Resources.appsettings.json");

        var config = new ConfigurationBuilder()
            .AddJsonStream(stream!)
            .Build();

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(config["sflicense"]);
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMarkup()
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
