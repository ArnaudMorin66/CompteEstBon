#region Copyright Syncfusion Inc. 2001 - 2024
// Copyright Syncfusion Inc. 2001 - 2024. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using QuickLook;

namespace  MauiCeb.Services ;
	

// ReSharper disable once InconsistentNaming
// ReSharper disable once CheckNamespace
public class PreviewControllerDS(QLPreviewItem item) : QLPreviewControllerDataSource
{
	private readonly QLPreviewItem _item = item;

	public override nint PreviewItemCount(QLPreviewController controller)
	{
		return 1;
	}

	public override IQLPreviewItem GetPreviewItem(QLPreviewController controller, nint index)
	{
		return _item;
	}
}