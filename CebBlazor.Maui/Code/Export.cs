//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Reflection;
using System.Runtime.InteropServices;

using CebBlazor.Maui.Services;

using CompteEstBon;

using Microsoft.JSInterop;

using Syncfusion.Blazor;

namespace CebBlazor.Maui.Code;

public static class Export {
	private static readonly Dictionary<string, string> ContentType = new() {
		["xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
		["json"] = "application/json",
		["docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
		["xml"]= "application/xml",
		["html"]= "text/html",
	};
	
	public static async Task SaveAsAsync(this IJSRuntime js, string filename, MemoryStream data) =>
		await js.InvokeVoidAsync("saveAsFile", filename, Convert.ToBase64String(data.ToArray()));

	public static string? SyncfusionVersion => typeof(SfBaseComponent).Assembly.GetName().Version?.ToString();

	public static string CebBlazorVersion => Assembly.GetExecutingAssembly().GetName().Version!.ToString();

	public static string DotNetVersion => RuntimeInformation.FrameworkDescription;

	public static string? Authors => "Arnaud Morin";

	public static string Version => $"{Authors} ({CebBlazorVersion}), Syncfusion {SyncfusionVersion}, {DotNetVersion}";

	public static async ValueTask ExportAsync( CebTirage tirage, string extension) {
		if (tirage.Status is not (CebStatus.CompteEstBon or CebStatus.CompteApproche)) return;
		var filename = $"CompteEstBon.{extension}";
		await using var mstream = new MemoryStream();
		Action<MemoryStream> exportStream = extension switch {
			"xlsx" => tirage.ExcelSaveStream,
			"docx" => tirage.WordStream,
			"json" => tirage.JsonSaveStream,
			"xml" => tirage.XmlSaveStream,
			"html" => tirage.HtmlStream,
			_ => throw new NotImplementedException()
		};
		exportStream(mstream);
		SaveService.SaveAndView(filename, ContentType[extension], mstream);
			
	}
}