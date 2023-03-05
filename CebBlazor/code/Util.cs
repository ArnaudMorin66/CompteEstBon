//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.JSInterop;
using Syncfusion.Blazor;

using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CebBlazor.Code;

public static class Util {
    public static async Task SaveAsAsync(this IJSRuntime js, string filename, MemoryStream data) =>
        await js.InvokeVoidAsync("saveAsFile", filename,  Convert.ToBase64String(data.ToArray()));

    public static string? SyncfusionVersion => typeof(SfBaseComponent).Assembly.GetName().Version?.ToString();

    public static string CebBlazorVersion => Assembly.GetExecutingAssembly().GetName().Version!.ToString();

    public static string DotNetVersion => RuntimeInformation.FrameworkDescription;

    public static string? Authors => (Assembly.GetEntryAssembly()?.GetCustomAttributes(
        typeof(AssemblyCopyrightAttribute),
        false)[0] as AssemblyCopyrightAttribute)?.Copyright;

    public static string Version => $"{Authors} ({CebBlazorVersion}), Syncfusion {SyncfusionVersion}, {DotNetVersion}";
   
}