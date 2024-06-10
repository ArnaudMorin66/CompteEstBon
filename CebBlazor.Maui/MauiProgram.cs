using CompteEstBon;
using CebBlazor.Maui.Code;
using CebBlazor.Maui.Properties;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

namespace CebBlazor.Maui;
public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();
            //.ConfigureFonts(fonts => {
            //    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            //});

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        builder.Services.AddScoped<CebTirage>();
        builder.Services.AddSyncfusionBlazor();
        builder.Services.AddSingleton(new CebSetting {
            MongoDb = bool.TryParse(builder.Configuration["mongodb:actif"], out var mdb) && mdb,
        MongoDbConnectionString = builder.Configuration["mongodb:server"],
        AutoCalcul = bool.TryParse(builder.Configuration["AutoCalcul"], out var res) && res
    });
        SyncfusionLicenseProvider.RegisterLicense(licenseKey: Resources.Licence);
        return builder.Build();
    }
}
