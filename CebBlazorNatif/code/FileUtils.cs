﻿using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CebBlazorNatif
{
    public static class FileUtils
    {
        public static async Task SaveAsAsync(this IJSRuntime jsRuntime, byte[] byteData, string mimeType, string fileName) {
            if (byteData == null) {
                await jsRuntime.InvokeVoidAsync("alert", "The byte array provided for Exporting was Null.");
            }
            else {
                await jsRuntime.InvokeVoidAsync("saveFile", Convert.ToBase64String(byteData), mimeType, fileName);
            }
        }
    }
}
