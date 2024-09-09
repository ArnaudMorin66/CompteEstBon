using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Syncfusion.Blazor;
using Syncfusion.Licensing;

namespace CebBlazor.Maui;

public static class MauiProgram {
	public static MauiApp CreateMauiApp() {
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts => {
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});
#if !ANDROID

			builder.Configuration.AddJsonFile("appsettings.json");
#endif
		builder.Services.AddMauiBlazorWebView();
		// builder.Services.AddScoped<CebTirage>();
#if ANDROID
		SyncfusionLicenseProvider.RegisterLicense("MzQyMTY2NkAzMjM2MmUzMDJlMzBhWE9sYmVsYkxDZFFkYll5VTRibDFyQ2NEWWQ5RFoybVhtemFpbld2Wm5RPQ==");
#else
			SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["sflicense"]);
#endif
		builder.Services.AddSyncfusionBlazor();

#if DEBUG
			builder.Services.AddBlazorWebViewDeveloperTools();
			builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}