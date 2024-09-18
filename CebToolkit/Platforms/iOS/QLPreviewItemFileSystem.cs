#region Copyright Syncfusion Inc. 2001 - 2024
// Copyright Syncfusion Inc. 2001 - 2024. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Foundation;
using QuickLook;
#pragma warning disable
// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
// ReSharper disable once InconsistentNaming
namespace CebToolkit.Services; 

public class QLPreviewItemFileSystem(string fileName, string filePath) : QLPreviewItem {
	public override string PreviewItemTitle => fileName;

	public override NSUrl PreviewItemUrl => NSUrl.FromFilename(filePath);
}

// ReSharper disable once InconsistentNaming
public class QLPreviewItemBundle(string fileName, string filePath) : QLPreviewItem {
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

