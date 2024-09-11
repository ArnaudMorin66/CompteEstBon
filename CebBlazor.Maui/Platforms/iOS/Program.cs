using CebBlazor.Maui.Platforms.iOS;
#pragma warning disable
using UIKit;

// ReSharper disable once CheckNamespace
namespace CebBlazor.Maui;

public class Program {
	// This is the main entry point of the application.
	static void Main(string[] args) {
		// if you want to use a different Application Delegate class from "AppDelegate"
		// you can specify it here.
		UIApplication.Main(args, null, typeof(AppDelegate));
	}
}