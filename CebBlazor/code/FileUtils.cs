using Microsoft.JSInterop;

// ReSharper disable once CheckNamespace
namespace CompteEstBon
{
    public static class FileUtils
    {
#pragma warning disable CRR0035
        public static ValueTask<object> SaveAsAsync(this IJSRuntime js, string filename, byte[] data)
            => js.InvokeAsync<object>(
                "saveAsFile",
                filename,
                Convert.ToBase64String(data));
       
    }
}
