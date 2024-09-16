#region Copyright Syncfusion Inc. 2001-2024.
// Copyright Syncfusion Inc. 2001-2024. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion

using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.ApplicationModel;

// ReSharper disable once CheckNamespace
namespace CebMaui.Services;

public static partial class SaveService
{
	public static async partial void SaveAndView(string filename, string contentType, MemoryStream stream)
	{
		StorageFile stFile;
		var extension = Path.GetExtension(filename);
		//Gets process windows handle to open the dialog in application process. 
		var windowHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
		if (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent($"Windows.Phone.UI.Input.HardwareButtons"))
		{
			//Creates file save picker to save a file. 
			FileSavePicker savePicker = new() { DefaultFileExtension = extension, SuggestedFileName = filename };
			savePicker.FileTypeChoices.Add(extension[1..].ToUpper(), (List<string>)[extension]);
				
			WinRT.Interop.InitializeWithWindow.Initialize(savePicker, windowHandle);
			stFile = await savePicker.PickSaveFileAsync();
		}
		else
		{
			var local = ApplicationData.Current.LocalFolder;
			stFile = await local.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
		}

		if (stFile == null) return;
		using (var zipStream = await stFile.OpenAsync(FileAccessMode.ReadWrite))
		{
			//Writes compressed data from memory to file.
			await using var outstream = zipStream.AsStreamForWrite();
			outstream.SetLength(0);
			//Saves the stream as file.
			var buffer = stream.ToArray();
			await outstream.WriteAsync(buffer);
			await outstream.FlushAsync();
		}
        
        
        
//Create message dialog box. 
        MessageDialog msgDialog = new("Voulez vous voir le document?", "Chargement terminé");
		msgDialog.Commands.Add(new UICommand("Oui",
			(_) => { Task.Run(() => Windows.System.Launcher.LaunchFileAsync(stFile)); }));
		UICommand noCmd = new("Non");
		msgDialog.Commands.Add(noCmd);

		WinRT.Interop.InitializeWithWindow.Initialize(msgDialog, windowHandle);
		//Showing a dialog box. 
		await msgDialog.ShowAsync();
		//if (cmd.Id == yesCmd.Id)
		//{
		//	//Launch the saved file. 
		//	await Windows.System.Launcher.LaunchFileAsync(stFile);
		//}
	}
}