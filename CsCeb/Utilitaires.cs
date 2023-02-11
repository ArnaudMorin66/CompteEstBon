//-----------------------------------------------------------------------
// <copyright file="Utilitaires.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.CommandLine.Rendering;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using static System.Console;

namespace CompteEstBon;

// ReSharper disable once CheckNamespace
/// <summary>
/// 
/// </summary>
public static class Utilitaires {
    /// <summary>
    /// 
    /// </summary>
    private static readonly Dictionary<string, Action<CebTirage, FileInfo>> listeFormats = new() {
        [".zip"] = SaveZip,
        [".json"] = SaveJson,
        [".xml"] = SaveXml,
        [".xlsx"] = SaveXlsx,
        [".docx"] = SaveDocx
    };
    /// <summary>
    /// 
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new() {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false
    };
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="file"></param>
    public static void SaveJson(this CebTirage tirage, FileInfo file) {
        using var stream = file.Create();
        tirage.JsonSaveStream(stream);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="file"></param>
    public static void SaveXlsx(this CebTirage tirage, FileInfo file) {
        using var stream = file.Create();
        tirage.ToExcel(stream);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="file"></param>
    public static void SaveDocx(this CebTirage tirage, FileInfo file) {
        using var stream = file.Create();
        tirage.ToWord(stream);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="file"></param>
    public static void SaveXml(this CebTirage tirage, FileInfo file) {
        using var stream = file.Create();
        tirage.XmlSaveStream(stream);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tirage"></param>
    public static void WriteJson(this CebTirage tirage) =>
        WriteLine(JsonSerializer.Serialize(tirage.Result, JsonOptions));
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="file"></param>
    public static void SaveZip(this CebTirage tirage, FileInfo file) {
        using var archive = ZipFile.Open(file.FullName, ZipArchiveMode.Update, Encoding.UTF8);
        var num = new[] { 0 }.Concat(
                archive.Entries.Select(
                    p => int.TryParse(p.Name[..p.Name.LastIndexOf('.')], out var result) ? result : 0))
            .Max();
        ZipStream(archive, $"{++num:000000}.json", tirage.JsonSaveStream);
        ZipStream(archive, $"{++num:000000}.xml", tirage.XmlSaveStream);
    }
    /// <summary>
    /// 
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
    /// 
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="stream"></param>
    public static void JsonSaveStream(this CebTirage tirage, Stream stream) => 
        JsonSerializer.Serialize(stream, tirage.Result, JsonOptions);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="stream"></param>
    /// <exception cref="Exception"></exception>
    public static void XmlSaveStream(this CebTirage tirage, Stream stream) {
        XmlSerializer mySerializer = new(typeof(CebData));
        try {
            mySerializer.Serialize(stream, tirage.Result);
        }
        catch (SerializationException) {
            throw new Exception("Erreur serialisation");
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="server"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task SerializeMongoAsync(this CebTirage tirage, string server) {
        try {
            ConventionRegistry.Register(
                "EnumStringConvention",
                new ConventionPack { new EnumRepresentationConvention(BsonType.String) },
                _ => true);
            var clientSettings = MongoClientSettings.FromConnectionString(server);
            clientSettings.LinqProvider = LinqProvider.V3;

            var cl = new MongoClient(clientSettings)
                .GetDatabase("CompteEstBon").GetCollection<BsonDocument>("c#");

            await cl.InsertOneAsync(
                new BsonDocument(
                    new Dictionary<string, object> {
                        {
                            "_id",
                            new {
                                domain = Environment.GetEnvironmentVariable("USERDOMAIN"),
                                date = DateTime.UtcNow
                            }.ToBsonDocument()
                        }
                    }).AddRange(tirage.Result.ToBsonDocument()));
        }
        catch (Exception) {
            throw new Exception("Erreur de sauvegarde sous MongoDb ");
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tirage"></param>
    /// <param name="fichiers"></param>
    /// <exception cref="Exception"></exception>
    public static void SerializeFichiers(this CebTirage tirage, List<FileInfo> fichiers) {
        foreach (var fichier in fichiers) {
            WriteLine($@"Exporter vers {fichier.FullName}".Cyan());
            if (listeFormats.TryGetValue(fichier.Extension, out var exportFichier)) {
                exportFichier(tirage, fichier);
            }
            else {
                throw new Exception("type de fichier non défini");
            }
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="texte"></param>
    /// <param name="bground"></param>
    /// <param name="eground"></param>
    /// <returns></returns>
    public static string TextControlCode(this object texte, AnsiControlCode bground, AnsiControlCode eground = null) =>
        $"{bground}{texte}{eground ?? Ansi.Color.Foreground.Default}";
    /// <summary>
    /// 
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Red(this object texte) => texte.TextControlCode(Ansi.Color.Foreground.Red);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string LightYellow(this object texte) => texte.TextControlCode(Ansi.Color.Foreground.LightYellow);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Cyan(this object texte) => texte.TextControlCode(Ansi.Color.Foreground.Cyan);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>

    public static string Green(this object texte) => texte.TextControlCode(Ansi.Color.Foreground.Green);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Magenta(this object texte) => texte.TextControlCode(Ansi.Color.Foreground.Magenta);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Yellow(this object texte) => texte.TextControlCode(Ansi.Color.Foreground.Yellow);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Blue(this object texte) => texte.TextControlCode(Ansi.Color.Foreground.Blue);
}