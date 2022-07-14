using Microsoft.JSInterop;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace CebBlazor.code {
#pragma warning disable CRR0048
    public enum CebTypeGrille {
        Grille,
        Liste
    }

    public static class FileUtils {
#pragma warning disable CRR0035
        public static ValueTask<object> SaveAsAsync(this IJSRuntime js, string filename, byte[] data)
            => js.InvokeAsync<object>(
                "saveAsFile",
                filename,
                Convert.ToBase64String(data));

    }
}
