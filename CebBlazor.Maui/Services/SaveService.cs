#region Copyright Syncfusion Inc. 2001-2024.
// Copyright Syncfusion Inc. 2001-2024. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion

namespace CebBlazor.Maui.Services
{
    public static partial class SaveService
    {
        //Method to save document as a file and view the saved document.
        public static partial void SaveAndView(string filename, string contentType, MemoryStream stream);
    }
}