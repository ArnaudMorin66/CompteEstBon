﻿using Microsoft.Extensions.Logging;

using Syncfusion.Maui.Core.Hosting;

namespace CebMaui;

public static class MauiProgram {

    public static MauiApp CreateMauiApp() {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
            Properties.Resource.SfLicense);
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