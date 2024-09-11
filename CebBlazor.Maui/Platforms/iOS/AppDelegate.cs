using Foundation;

namespace CebBlazor.Maui.Platforms.iOS;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate {
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}