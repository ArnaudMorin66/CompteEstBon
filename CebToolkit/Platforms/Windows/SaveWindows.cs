#region Copyright Syncfusion Inc. 2001-2024.
// Copyright Syncfusion Inc. 2001-2024. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion

using CommunityToolkit.Maui.Storage;
using System.IO;

using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

// ReSharper disable once CheckNamespace
namespace CebToolkit.Services;

public static partial class SaveService
{
    public static async partial void SaveAndView(string filename, string contentType, MemoryStream stream) {
        
        var extension = Path.GetExtension(filename);
        //Gets process windows handle to open the dialog in application process. 
        var windowHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        var fileresult = await FileSaver.Default.SaveAsync(filename, stream);

        if (fileresult.IsSuccessful) {

            StorageFile file = await StorageFile.GetFileFromPathAsync(filename);

//Create message dialog box. 
            MessageDialog msgDialog = new("Voulez vous voir le document?", "Chargement terminé");
            msgDialog.Commands.Add(new UICommand("Oui",
                (_) => { Task.Run(() => Windows.System.Launcher.LaunchFileAsync(file)); }));
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
}