//-----------------------------------------------------------------------
// <copyright file="CebSerialize.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using Syncfusion.DocIO;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Xml.Serialization;

namespace CompteEstBon;

public static class CebSerialize {
    public static readonly Dictionary<string, Action<CebTirageBase, FileInfo>> ListeFormats =
        new() {
            [".zip"] = SaveFileZip,
            [".json"] = SaveFileJson,
            [".xml"] = SaveFileXml,
            [".xlsx"] = SaveFileExcel,
            [".docx"] = SaveFileWord,
            [".html"] = SaveFileWord
        };

    /// <summary>
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new() {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = {
            new JsonStringEnumConverter()
        },
        WriteIndented = true
    };

    public static readonly Dictionary<string, Action<CebTirageBase, Stream>> ListeStreamFormats =
        new() {
            ["json"] = SaveStreamJson,
            ["xml"] = SaveStreamXml,
            ["xlsx"] = ExportOffice.SaveStreamExcel,
            ["docx"] = ExportOffice.SaveStreamWord,
            ["html"] = ExportOffice.SaveStreamHtml
        };

    public static bool Export(this CebTirageBase tirage, string ext, Stream stream) {
        if (!ListeStreamFormats.TryGetValue(ext, out var action)) return false;
        action(tirage, stream);
        return true;
    }

    public static bool Export(this CebTirageBase tirage, FileInfo fi) {
        if (!ListeFormats.TryGetValue(fi.Extension, out var laction)) return false;
        if (fi.Exists) fi.Delete();
        laction(tirage, fi);
        return true;
    }

    public static bool Export(this CebTirageBase tirage, string path) => Export(tirage, new FileInfo(path));

    /// <summary>
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="file"></param>
    public static void SaveFileJson(this CebTirageBase tirage, FileInfo file) {
        using var stream = file.Create();
        tirage.SaveStreamJson(stream);
    }

    /// <summary>
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="file"></param>
    public static void SaveFileExcel(this CebTirageBase tirage, FileInfo file) {
        using var stream = file.Create();
        tirage.SaveStreamExcel(stream);
    }

    /// <summary>
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="file"></param>
    public static void SaveFileWord(this CebTirageBase tirage, FileInfo file) {
        using var stream = file.Create();
        tirage.SaveStreamWordType(stream, file.Extension == ".html" ? FormatType.Html : FormatType.Docx);
    }

    /// <summary>
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="file"></param>
    public static void SaveFileXml(this CebTirageBase tirage, FileInfo file) {
        using var stream = file.Create();
        tirage.SaveStreamXml(stream);
    }

    /// <summary>
    /// </summary>
    /// <param name="tirage"></param>
    public static string WriteJson(this CebTirageBase tirage) => JsonSerializer.Serialize(tirage.Resultat, JsonOptions);

    /// <summary>
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="file"></param>
    public static void SaveFileZip(this CebTirageBase tirage, FileInfo file) {
        using var archive = ZipFile.Open(file.FullName, ZipArchiveMode.Update, Encoding.UTF8);
        var num = new[] { 0 }.Concat(
                archive.Entries.Select(
                    p => int.TryParse(p.Name[..p.Name.LastIndexOf('.')], out var result) ? result : 0))
            .Max();
        ZipStream(archive, $"{++num:000000}.json", tirage.SaveStreamJson);
        ZipStream(archive, $"{++num:000000}.xml", tirage.SaveStreamXml);
    }

    /// <summary>
    /// </summary>
    /// <param name="archive"></param>
    /// <param name="nom"></param>
    /// <param name="action"></param>
    public static void ZipStream(ZipArchive archive, string nom, Action<Stream> action) {
        var stream = archive.CreateEntry(nom, CompressionLevel.SmallestSize).Open();
        action(stream);
        stream.Close();
    }

    /// <summary>
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="stream"></param>
    public static void SaveStreamJson(this CebTirageBase tirage, Stream stream) => JsonSerializer.Serialize(
        stream,
        tirage.Resultat,
        JsonOptions);

    /// <summary>
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="stream"></param>
    /// <exception cref="Exception"></exception>
    public static void SaveStreamXml(this CebTirageBase tirage, Stream stream) {
        XmlSerializer mySerializer = new(typeof(CebData));
        try {
            mySerializer.Serialize(stream, tirage.Resultat);
        } catch (SerializationException) {
            throw new Exception("Erreur serialisation");
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="fichiers"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static void SerializeFichiers(this CebTirageBase tirage, IEnumerable<FileInfo> fichiers) {
        foreach (var fichier in fichiers) tirage.Export(fichier);
    }
}