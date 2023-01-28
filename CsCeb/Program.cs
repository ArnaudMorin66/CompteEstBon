//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.CommandLine;
using System.CommandLine.Rendering;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using CompteEstBon;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using static System.Console;

// ReSharper disable LocalizableElement


var Json = false;
var Save = false;
var ConfigurationFile =
    @$"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Ceb\config.json";
var MongoServer = string.Empty;
var SaveToMongoDb = false;
FileInfo fichier = null;
CebTirage tirage = new();

WriteLine('\n');
WriteLine("*** Le Compte est bon ***".Red());
WriteLine();
ConfigurationBuilder builder = new();
if (File.Exists(ConfigurationFile)) {
    builder.AddJsonFile(ConfigurationFile);
    var jbuild = builder.Build();

    foreach (var child in jbuild.GetChildren())
        switch (child.Path.ToUpper()) {
            case "SAVE":
                Save = bool.TryParse(child.Value, out var v) && v;
                break;
            case "MONGODB":
                SaveToMongoDb = bool.TryParse(child.Value, out var wv) && wv;
                break;
            case "MONGODBSERVER":
                MongoServer = child.Value;
                break;
            case "PLATFORM":
                foreach (var plt in child.GetChildren())
                    if (plt["os"] == "win32") {
                        fichier = new FileInfo(plt["ZipFile"]);
                        break;
                    }

                break;
        }
}


var rootCommand = new RootCommand("Compte Est Bon") {
    new Option<int>(new[] { "--trouve", "-t" }, "Nombre à chercher"),
    new Option<int[]>(new[] { "--plaques", "-p" }, "Liste des plaques") { AllowMultipleArgumentsPerToken = true },
    new Option<bool>(new[] { "--json", "-j" }, "Export au format JSON"),
    new Option<bool>(new[] { "--sauvegarde", "-s" }, "Sauvegarder le Compte"),
    new Option<bool>(new[] { "--mongodb", "-m" }, "Sauvegarder le Compte dans MongoDB"),
    new Option<string>(new[] { "--serveur", "-S" }, "Nom du serveur MongoDB"),
    new Option<FileInfo>(new[] { "--fichier", "-f" }, "Fichier"),
    new Argument<int[]>("arguments", "Plaques et nombre à trouver")
};

rootCommand.SetHandler(async context => {
    var prs = context.ParseResult;

    foreach (var option in rootCommand.Options)
        if (prs.FindResultFor(option) is { } optionResult)
            switch (option.Name.ToLower()) {
                case "trouve":
                    tirage.Search = optionResult.GetValueOrDefault<int>();
                    break;

                case "plaques":
                    tirage.SetPlaques(optionResult.GetValueOrDefault<int[]>());
                    break;

                case "json":
                    Json = optionResult.GetValueOrDefault<bool>();
                    break;

                case "fichier":
                    fichier = optionResult.GetValueOrDefault<FileInfo>();
                    break;

                case "sauvegarde":
                    Save = optionResult.GetValueOrDefault<bool>();
                    break;

                case "mongodb":
                    SaveToMongoDb = optionResult.GetValueOrDefault<bool>();
                    break;

                case "serveur":
                    MongoServer = optionResult.GetValueOrDefault<string>();
                    break;

                default:
                    AbortProgramme($"Argument {option.Name} invalide");
                    break;
            }


    if (prs.FindResultFor(rootCommand.Arguments[0]) is { } argumentResult) {
        var arguments = argumentResult.GetValueOrDefault<int[]>();

        if (arguments.Length > 7) AbortProgramme("Nombre d'arguments invalide");

        if (arguments[0] > 100) {
            tirage.Search = arguments[0];
            arguments = arguments[1..]; 
            
        }
        else if (arguments.Length == 7 && arguments[6] > 100) {
            tirage.Search = arguments[6];
            arguments = arguments[..6]; 
        }

        if (arguments.Length == 6)
            tirage.SetPlaques(arguments);
        else if (arguments.Length > 0) AbortProgramme("Arguments invalide");
    }

    if (Json) {
        tirage.Resolve();
        WriteLine(JsonSerializer.Serialize(tirage.Data, JsonOptions()));
    }
    else {
       
        Write("Tirage:\t".Yellow());
        Write("Plaques: ".LightYellow());
        foreach (var plaque in tirage.Plaques) Write($@"{plaque} ");
        Write("Recherche: ".LightYellow());
        Write(tirage.Search);

        WriteLine();
        var result = tirage.Resolve();

        WriteLine();

        if (tirage.Status == CebStatus.Invalide) AbortProgramme("Tirage  invalide");

        var txtStatus = result == CebStatus.CompteEstBon ? result.Green() : result.Magenta();
        Write(txtStatus);
        if (result == CebStatus.CompteApproche) Write($": {tirage.Found}");

        Write($", {"nombre de solutions:".LightYellow()} {tirage.Solutions.Count}");
        WriteLine($", {"Durée du calcul:".LightYellow()} {tirage.Duree:F3} s");

        WriteLine();

        foreach (var (i, solution) in tirage.Solutions.Select((v, i) => (i, v))) {
            var count = $"{i + 1:0000}".ColorForeground(
                tirage.Status == CebStatus.CompteEstBon ? Ansi.Color.Foreground.Green : Ansi.Color.Foreground.Magenta);
            WriteLine($"{solution.Rank} - {count}: {solution.LightYellow()}");
        }

        WriteLine();
    }

    if (SaveToMongoDb && MongoServer != string.Empty) await SaveMongoDB();

    if (Save == false) Environment.Exit(0);

    if (fichier != null && Save)
        if (fichier.Extension == ".zip")
            SaveZip();
        else
            SaveJson();
});

await rootCommand.InvokeAsync(args);
WriteLine();

void AbortProgramme(string message) {
    WriteLine($"{"Erreur: ".Red()}{message}".LightYellow());
    Environment.Exit(-1);
}
void SaveJson() {
    using var stream = fichier.Create();
    JsonSaveStream(stream);
}

void SaveZip() {
    using var archive = ZipFile.Open(fichier.FullName, ZipArchiveMode.Update, Encoding.UTF8);
    var num = new[] { 0 }.Concat(
            archive.Entries.Select(p => int.TryParse(p.Name[..p.Name.LastIndexOf('.')], out var result) ? result : 0))
        .Max();
    SaveStream(archive, $"{++num:000000}.json", JsonSaveStream);
    SaveStream(archive, $"{++num:000000}.xml", XmlSaveStream);
}

void SaveStream(ZipArchive archive, string nom, Action<Stream> action) {
    var stream = archive.CreateEntry(nom, CompressionLevel.SmallestSize).Open();
    action(stream);
    stream.Close();
}

void JsonSaveStream(Stream stream) => JsonSerializer.Serialize(stream, tirage.Data, JsonOptions());

void XmlSaveStream(Stream stream) {
    XmlSerializer mySerializer = new(typeof(CebData));
    try {
        mySerializer.Serialize(stream, tirage.Data);
    }
    catch (SerializationException) {
        AbortProgramme("Erreur serialisation");
    }
}

JsonSerializerOptions JsonOptions() =>
    new() {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false
    };

async Task SaveMongoDB() {
    try {
        ConventionRegistry.Register(
            "EnumStringConvention",
            new ConventionPack { new EnumRepresentationConvention(BsonType.String) },
            _ => true);
        var clientSettings = MongoClientSettings.FromConnectionString(MongoServer);
        clientSettings.LinqProvider = LinqProvider.V3;

        var cl = new MongoClient(clientSettings)
            .GetDatabase("ceb")
            .GetCollection<BsonDocument>("comptes");
        await cl.InsertOneAsync(
            new BsonDocument(
                    new Dictionary<string, object> {
                        {
                            "_id",
                            new {
                                lang = "c#", domain = Environment.GetEnvironmentVariable("USERDOMAIN"),
                                date = DateTime.UtcNow
                            }.ToBsonDocument(
                            )
                        }
                    })
                .AddRange(tirage.Data.ToBsonDocument()));
    }
    catch (Exception) {
        AbortProgramme("Erreur de sauvegarde sous MongoDb ");
    }
}


internal static class Colorizer {
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