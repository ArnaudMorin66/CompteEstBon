﻿//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="">
//     Author:  Arnaud Morin
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Reflection;
using System.Runtime.InteropServices;

using CompteEstBon;

using Microsoft.JSInterop;

using Syncfusion.Blazor;

namespace CebBlazor.Code;

public static class Util {
  public static async Task SaveAsAsync(this IJSRuntime js, string filename, MemoryStream data) =>
    await js.InvokeVoidAsync("saveAsFile", filename, Convert.ToBase64String(data.ToArray()));

  public static string? SyncfusionVersion => typeof(SfBaseComponent).Assembly.GetName().Version?.ToString();

  public static string CebBlazorVersion => Assembly.GetExecutingAssembly().GetName().Version!.ToString();

  public static string DotNetVersion => RuntimeInformation.FrameworkDescription;

  public static string? Authors => "Arnaud Morin";
  //(Assembly.GetEntryAssembly()?.GetCustomAttributes(
  //    typeof(AssemblyCopyrightAttribute),
  //    false)[0] as AssemblyCopyrightAttribute)?.Copyright;

  public static string Version => $"{Authors} ({CebBlazorVersion}), Syncfusion {SyncfusionVersion}, {DotNetVersion}";

  public static async ValueTask ExportAsync(this IJSRuntime js, CebTirage tirage, string extension) {
    if (tirage.Status is not (CebStatus.CompteEstBon or CebStatus.CompteApproche)) return;
    var filename = $"CompteEstBon.{extension}";
    await using var mstream = new MemoryStream();
    Action<MemoryStream> exportStream = extension switch {
      "xlsx" => tirage.SaveStreamExcel,
      "docx" => tirage.SaveStreamWord,
      "json" => tirage.SaveStreamJson,
      "xml" => tirage.SaveStreamXml,
      "html" => tirage.SaveStreamHtml,
      _ => throw new NotImplementedException()
    };
    exportStream(mstream);
    await js.SaveAsAsync(filename, mstream);
  }
}