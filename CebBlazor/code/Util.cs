//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Microsoft.JSInterop;

// ReSharper disable once CheckNamespace
namespace CebBlazor.Code;

public static class Util {
#pragma warning disable CRR0035
    public static ValueTask<object> SaveAsAsync(this IJSRuntime js, string filename, byte[] data) => js.InvokeAsync<object>(
        "saveAsFile",
        filename,
        Convert.ToBase64String(data));
}
