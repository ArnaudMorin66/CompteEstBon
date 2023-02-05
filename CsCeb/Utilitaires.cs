//-----------------------------------------------------------------------
// <copyright file="Utilitaires.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.CommandLine.Rendering;
using System.Diagnostics;
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
public static class Utilitaires {
    private static readonly Dictionary<string, Action<CebTirage, FileInfo>> listeFormats = new() {
        [".zip"] = SaveZip,
        [".json"] = SaveJson,
        [".xml"] = SaveXml,
        [".xlsx"] = SaveXlsx,
        [".docx"] = SaveDocx
    };

    public static readonly JsonSerializerOptions JsonOptions = new() {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false
    };

    public static void SaveJson(this CebTirage tirage, FileInfo file) {
        using var stream = file.Create();
        tirage.JsonSaveStream(stream);
    }

    public static void SaveXlsx(this CebTirage tirage, FileInfo file) {
        using var stream = file.Create();
        tirage.ToExcel(stream);
    }

    public static void SaveDocx(this CebTirage tirage, FileInfo file) {
        using var stream = file.Create();
        tirage.ToWord(stream);
    }

    public static void SaveXml(this CebTirage tirage, FileInfo file) {
        using var stream = file.Create();
        tirage.XmlSaveStream(stream);
    }

    public static void WriteJson(this CebTirage tirage) =>
        WriteLine(JsonSerializer.Serialize(tirage.Result, JsonOptions));

    public static void SaveZip(this CebTirage tirage, FileInfo file) {
        using var archive = ZipFile.Open(file.FullName, ZipArchiveMode.Update, Encoding.UTF8);
        var num = new[] { 0 }.Concat(
                archive.Entries.Select(
                    p => int.TryParse(p.Name[..p.Name.LastIndexOf('.')], out var result) ? result : 0))
            .Max();
        ZipStream(archive, $"{++num:000000}.json", tirage.JsonSaveStream);
        ZipStream(archive, $"{++num:000000}.xml", tirage.XmlSaveStream);
    }

    public static void ZipStream(ZipArchive archive, string nom, Action<Stream> action) {
        var stream = archive.CreateEntry(nom, CompressionLevel.SmallestSize).Open();
        action(stream);
        stream.Close();
    }

    public static void JsonSaveStream(this CebTirage tirage, Stream stream) => 
        JsonSerializer.Serialize(stream, tirage.Result, JsonOptions);

    public static void XmlSaveStream(this CebTirage tirage, Stream stream) {
        XmlSerializer mySerializer = new(typeof(CebData));
        try {
            mySerializer.Serialize(stream, tirage.Result);
        }
        catch (SerializationException) {
            throw new Exception("Erreur serialisation");
        }
    }


    public static async Task SerializeTirageMongoAsync(this CebTirage tirage, string server) {
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

    public static void OpenDocument(string nom) =>
        Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = nom });

    public static string ColorForeground(this object texte, AnsiControlCode bground, AnsiControlCode eground = null) =>
        $"{bground}{texte}{eground ?? Ansi.Color.Foreground.Default}";

    public static string Red(this object texte) => texte.ColorForeground(Ansi.Color.Foreground.Red);

    public static string LightYellow(this object texte) => texte.ColorForeground(Ansi.Color.Foreground.LightYellow);

    public static string Cyan(this object texte) => texte.ColorForeground(Ansi.Color.Foreground.Cyan);


    public static string Green(this object texte) => texte.ColorForeground(Ansi.Color.Foreground.Green);

    public static string Magenta(this object texte) => texte.ColorForeground(Ansi.Color.Foreground.Magenta);

    public static string Yellow(this object texte) => texte.ColorForeground(Ansi.Color.Foreground.Yellow);

    public static string Blue(this object texte) => texte.ColorForeground(Ansi.Color.Foreground.Blue);
}