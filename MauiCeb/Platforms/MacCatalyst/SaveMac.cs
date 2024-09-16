#region Copyright Syncfusion Inc. 2001-2024.
// Copyright Syncfusion Inc. 2001-2024. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Foundation;
using QuickLook;

using UIKit;
#pragma warning disable
namespace MauiCeb.Services
{
	public static partial class SaveService
	{
		public static partial void SaveAndView(string filename, string contentType, MemoryStream stream)
		{
			var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var filePath = Path.Combine(path, filename);
			stream.Position = 0;
			//Saves the document
			using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.ReadWrite);
			stream.CopyTo(fileStream);
			fileStream.Flush();
			
			var window = GetKeyWindow();
			if (window is { RootViewController: not null })
			{
				var uiViewController = window.RootViewController;
				if (uiViewController != null)
				{
					QLPreviewController qlPreview = [];
					QLPreviewItem item = new QlPreviewItemBundle(filename, filePath);
					qlPreview.DataSource = new PreviewControllerDs(item);
					uiViewController.PresentViewController(qlPreview, true, null);
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

public class QlPreviewItemFileSystem(string fileName, string filePath) : QLPreviewItem {
	public override string PreviewItemTitle => fileName;

	public override NSUrl PreviewItemUrl => NSUrl.FromFilename(filePath);
}

public class QlPreviewItemBundle(string fileName, string filePath) : QLPreviewItem {
	public override string PreviewItemTitle => fileName;

	public override NSUrl PreviewItemUrl
	{
		get
		{
			var documents = NSBundle.MainBundle.BundlePath;
			var lib = Path.Combine(documents, filePath);
			var url = NSUrl.FromFilename(lib);
			return url;
		}
	}
}

public class PreviewControllerDs(QLPreviewItem item) : QLPreviewControllerDataSource {
	public override nint PreviewItemCount(QLPreviewController controller)
	{
		return 1;
	}

	public override IQLPreviewItem GetPreviewItem(QLPreviewController controller, nint index)
	{
		return item;
	}
}