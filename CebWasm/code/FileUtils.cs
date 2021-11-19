using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace CebWasm
{
    public static class FileUtils
    {               
        public static async Task SaveAsAsync(this IJSRuntime jsRuntime, byte[] byteData, string mimeType, string fileName) {
            if (byteData == null) {
                await jsRuntime.InvokeVoidAsync("alert", "Le fichier à exporter est non défini.");
            }
            else {
                await jsRuntime.InvokeVoidAsync("saveFile", Convert.ToBase64String(byteData), mimeType, fileName);
            }
        }
    }
}
