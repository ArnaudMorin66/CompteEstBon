//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Reflection;
using System.Runtime.InteropServices;

using CompteEstBon;

using Microsoft.JSInterop;

using Syncfusion.Blazor;

namespace CebBlazor.Maui.Code;

public static class Util {
    public static async Task SaveAsAsync(this IJSRuntime js, string filename, MemoryStream data) =>
        await js.InvokeVoidAsync("saveAsFile", filename, Convert.ToBase64String(data.ToArray()));

#pragma warning disable CS8632 // L'annotation pour les types référence Nullable doit être utilisée uniquement dans le code au sein d'un contexte d'annotations '#nullable'.
    public static string? SyncfusionVersion => typeof(SfBaseComponent).Assembly.GetName().Version?.ToString();
#pragma warning restore CS8632 // L'annotation pour les types référence Nullable doit être utilisée uniquement dans le code au sein d'un contexte d'annotations '#nullable'.

    public static string CebBlazorVersion => Assembly.GetExecutingAssembly().GetName().Version!.ToString();

    public static string DotNetVersion => RuntimeInformation.FrameworkDescription;

#pragma warning disable CS8632 // L'annotation pour les types référence Nullable doit être utilisée uniquement dans le code au sein d'un contexte d'annotations '#nullable'.
    public static string? Authors => (Assembly.GetEntryAssembly()?.GetCustomAttributes(
        typeof(AssemblyCopyrightAttribute),
        false)[0] as AssemblyCopyrightAttribute)?.Copyright;
#pragma warning restore CS8632 // L'annotation pour les types référence Nullable doit être utilisée uniquement dans le code au sein d'un contexte d'annotations '#nullable'.
    
    public static string Version => $"{Authors} ({CebBlazorVersion}), Syncfusion {SyncfusionVersion}, {DotNetVersion}";

    public static async ValueTask ExportAsync(this IJSRuntime js, CebTirage tirage, string extension) {
        if (tirage.Status is not (CebStatus.CompteEstBon or CebStatus.CompteApproche)) return;
        var filename = $"CompteEstBon.{extension}";
        await using var mstream = new MemoryStream();
        Action<MemoryStream> exportStream = extension switch {
            "xlsx" => tirage.ExcelSaveStream,
            "docx" => tirage.WordSaveStream,
            "json" => tirage.JsonSaveStream,
            "xml" => tirage.XmlSaveStream,
            _ => throw new NotImplementedException()
        };
        exportStream(mstream);
        await js.SaveAsAsync(filename, mstream);
    }
}