﻿//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.JSInterop;
using Syncfusion.Blazor;
using System.Reflection;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace CebBlazor.Code;

public static class Util {
#pragma warning disable CRR0035
    public static ValueTask<object> SaveAsAsync(this IJSRuntime js, string filename, MemoryStream data) {
        return js.InvokeAsync<object>("saveAsFile", filename, Convert.ToBase64String(data.ToArray()));
    }

    public static string? SyncfusionVersion => typeof(SfBaseComponent).Assembly.GetName().Version?.ToString();

#pragma warning disable CS8602 // Déréférencement d'une éventuelle référence null.
    public static string CebBlazorVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
#pragma warning restore CS8602 // Déréférencement d'une éventuelle référence null.

    public static string DotNetVersion => RuntimeInformation.FrameworkDescription;

    public static string? Authors => (Assembly.GetEntryAssembly()?.GetCustomAttributes(
        typeof(AssemblyCopyrightAttribute),
        false)[0] as AssemblyCopyrightAttribute)?.Copyright;

    public static string Version => $"{Authors} ({CebBlazorVersion}), Syncfusion {SyncfusionVersion}, {DotNetVersion}";
}