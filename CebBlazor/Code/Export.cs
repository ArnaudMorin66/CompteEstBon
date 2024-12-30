//-----------------------------------------------------------------------
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

/// <summary>
/// Classe utilitaire pour diverses opérations.
/// </summary>
public static class Util {
    /// <summary>
    /// Sauvegarde un fichier en utilisant IJSRuntime.
    /// </summary>
    /// <param name="js">Instance de IJSRuntime.</param>
    /// <param name="filename">Nom du fichier.</param>
    /// <param name="data">Données du fichier.</param>
    /// <returns>Une tâche asynchrone.</returns>
    public static async Task SaveAsAsync(this IJSRuntime js, string filename, MemoryStream data) =>
      await js.InvokeVoidAsync("saveAsFile", filename, Convert.ToBase64String(data.ToArray()));

    /// <summary>
    /// Obtient la version de Syncfusion.
    /// </summary>
    public static string? SyncfusionVersion => typeof(SfBaseComponent).Assembly.GetName().Version?.ToString();

    /// <summary>
    /// Obtient la version de CebBlazor.
    /// </summary>
    public static string CebBlazorVersion => Assembly.GetExecutingAssembly().GetName().Version!.ToString();

    /// <summary>
    /// Obtient la version de .NET.
    /// </summary>
    public static string DotNetVersion => RuntimeInformation.FrameworkDescription;

    /// <summary>
    /// Obtient les auteurs.
    /// </summary>
    public static string? Authors => "Arnaud Morin";

    /// <summary>
    /// Obtient la version complète incluant les informations de Syncfusion et .NET.
    /// </summary>
    public static string Version => $"{Authors} ({CebBlazorVersion}), Syncfusion {SyncfusionVersion}, {DotNetVersion}";

    /// <summary>
    /// Exporte un tirage en utilisant IJSRuntime.
    /// </summary>
    /// <param name="js">Instance de IJSRuntime.</param>
    /// <param name="tirage">Instance de CebTirage.</param>
    /// <param name="extension">Extension du fichier.</param>
    /// <returns>Une tâche asynchrone.</returns>
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
