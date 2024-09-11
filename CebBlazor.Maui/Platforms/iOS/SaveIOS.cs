#region Copyright Syncfusion Inc. 2001 - 2024
// Copyright Syncfusion Inc. 2001 - 2024. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion

using CebBlazor.Maui.Platforms.iOS;

using QuickLook;

using UIKit;

// ReSharper disable once CheckNamespace
namespace CebBlazor.Maui.Services
{
	public static partial class SaveService
	{
		public  static partial void SaveAndView(string filename, string contentType, MemoryStream stream)
		{
			var exception = string.Empty;
			var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var filePath = Path.Combine(path, filename);
			try
			{
				var fileStream = File.Open(filePath, FileMode.Create);
				stream.Position = 0;
				stream.CopyTo(fileStream);
				fileStream.Flush();
				fileStream.Close();
			}
			catch (Exception e)
			{
				exception = e.ToString();
			}
			if (contentType != "application/html" || exception == string.Empty)
			{
				var window = GetKeyWindow();
				if (window is { RootViewController: not null })
				{
					var uiViewController = window.RootViewController;
					if (uiViewController != null)
					{
						QLPreviewController qlPreview = [];
						QLPreviewItem item = new QLPreviewItemBundle(filename, filePath);
						qlPreview.DataSource = new PreviewControllerDS(item);
						uiViewController.PresentViewController(qlPreview, true, null);
					}
				}
			}
		}
		public static UIWindow? GetKeyWindow()
		{
			foreach (var scene in UIApplication.SharedApplication.ConnectedScenes)
			{
				if (scene is UIWindowScene windowScene)
				{
					foreach (var window in windowScene.Windows)
					{
						if (window.IsKeyWindow)
						{
							return window;
						}
					}
				}
			}
			return null;
		}
	}
}
