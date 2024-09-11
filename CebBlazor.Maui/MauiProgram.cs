using Microsoft.Extensions.Logging;

using Syncfusion.Blazor;
using Syncfusion.Licensing;
using CebBlazor.Maui.Properties;

namespace CebBlazor.Maui;


public static class MauiProgram {
	public static MauiApp CreateMauiApp() {
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts => {
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});
		
		
		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSyncfusionBlazor();

#if DEBUG
			builder.Services.AddBlazorWebViewDeveloperTools();
			builder.Logging.AddDebug();
#endif
		SyncfusionLicenseProvider.RegisterLicense(Resources.SfLicence);
		return builder.Build();
	}
}