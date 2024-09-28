#region Copyright Syncfusion Inc. 2001-2024.
// Copyright Syncfusion Inc. 2001-2024. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Android.Content;
using Android.OS;
using Java.IO;

// ReSharper disable once CheckNamespace
namespace CebToolkit.Services;

public static partial class SaveService
{
	public static partial void SaveAndView(string filename, string contentType, MemoryStream stream)
	{
		var root = Android.OS.Environment.IsExternalStorageEmulated ? Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads)!.AbsolutePath : System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

		Java.IO.File myDir = new(root + "/CebDictionary");
		myDir.Mkdir();

		Java.IO.File file = new(myDir, filename);

		if (file.Exists())
		{
			file.Delete();
		}

		try
		{
			FileOutputStream outs = new(file);
			outs.Write(stream.ToArray());

			outs.Flush();
			outs.Close();
		} catch (Exception ) {
			// ignored
		}

		if (file.Exists())
		{

			if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N)
			{
				var fileUri = AndroidX.Core.Content.FileProvider.GetUriForFile(Android.App.Application.Context, Android.App.Application.Context.PackageName + ".provider", file);
				var intent = new Intent(Intent.ActionView);
				intent.SetData(fileUri);
				intent.AddFlags(ActivityFlags.NewTask);
				intent.AddFlags(ActivityFlags.GrantReadUriPermission);
				Android.App.Application.Context.StartActivity(intent);
			}
			else
			{
				var fileUri = Android.Net.Uri.Parse(file.AbsolutePath);
				var intent = new Intent(Intent.ActionView);
				intent.SetDataAndType(fileUri, contentType);
				intent = Intent.CreateChooser(intent, "Open File");
				intent!.AddFlags(ActivityFlags.NewTask);
				Android.App.Application.Context.StartActivity(intent);
			}

		}
	}
}