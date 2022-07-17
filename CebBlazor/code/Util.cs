using Microsoft.JSInterop;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace CebBlazor.code {
      public static class FileUtils {
        public static ValueTask<object> SaveAsAsync(this IJSRuntime js, string filename, byte[] data)
            => js.InvokeAsync<object>(
                "saveAsFile",
                filename,
                Convert.ToBase64String(data));

    }
}
