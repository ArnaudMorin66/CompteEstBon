using CompteEstBon;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using static System.Console;

/// <summary>
/// </summary>

bool Json = false;
bool Save = false;
string ConfigurationFile = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Ceb\\config.json";
string MongoServer = string.Empty;
bool SaveToMongoDb = false;
FileInfo fichier = null;

CebTirage tirage = new();

ConfigurationBuilder builder = new();
if (File.Exists(ConfigurationFile)) {
    builder.AddJsonFile(ConfigurationFile);
    var jbuild = builder.Build();
    var vfichier = jbuild["win32:ZipFile"];
    if (!string.IsNullOrEmpty(vfichier))
        fichier = new(vfichier);
    Save = bool.Parse(jbuild["Save"]);
    SaveToMongoDb = bool.Parse(jbuild["MongoDB"]);
    MongoServer = jbuild["MongoDBServer"];
}

RootCommand rootCommand = new() {
    new Option<int>(new[] { "--search", "-s" }, description: "Nombre à chercher"),
    new Option<int[]>(new[] { "--plaques", "-p" }, description: "Liste des plaques"),
    new Option<bool>(new[] { "--json", "-j" }, description: "Export au format JSON"),
    new Option<bool>(new[] { "--save", "-S" }, description: "Sauvegarder le Compte"),
    new Option<bool>(new[] { "--mongodb", "-M" }, description: "Sauvegarder le Compte dans MongoDB"),
    new Option<string>(new[] { "--server", "-B" }, description: "Nom du serveur MongoDB"),
    new Option<FileInfo>(new[] { "--file", "-f" }, description: "Fichier"),
    new Argument<int[]>("arguments", description: "Liste des plaques et nombre à chercher")
};
rootCommand.Description = "Compte est bon";

await rootCommand.InvokeAsync(args);

var prs = rootCommand.Parse(args);

if (prs.Errors.Any()) Environment.Exit(-1);

if (prs.FindResultFor(rootCommand.First(p => p.Name == "help")) != null) Environment.Exit(0);
if (prs.FindResultFor(rootCommand.First(p => p.Name == "version")) != null) Environment.Exit(0);

foreach (var option in rootCommand.Options.Where(p => prs.FindResultFor(p) != null)) {
    switch (option.Name) {
        case "search":
            tirage.Search = prs.ValueForOption<int>(option);
            break;

        case "plaques":
            tirage.SetPlaques(prs.ValueForOption<int[]>(option));
            break;

        case "json":
            Json = prs.ValueForOption<bool>(option);
            break;

        case "file":
            fichier = prs.ValueForOption<FileInfo>(option);
            break;

        case "version":
        case "help":
            Environment.Exit(0);
            break;

        case "save":
            Save = prs.ValueForOption<bool>(option);
            break;

        case "mongodb":
            SaveToMongoDb = prs.ValueForOption<bool>(option);
            break;

        case "server":
            MongoServer = prs.ValueForOption<string>(option);
            break;

        default:
            Environment.Exit(-1);
            break;
    }
};

var arguments = prs.ValueForArgument<int[]>(rootCommand.Arguments.First(p => p.Name == "arguments"));
if (arguments.Length > 0) {
    if (arguments[0] == 0 && arguments.Length == 1) {
        WriteLine("Paramètre invalide".Red());
        Environment.Exit(-1);
    }

    if (arguments[0] > 100) {
        tirage.Search = arguments[0];
        arguments = arguments.Where((_, i) => i > 0).ToArray();
    }
    if (arguments.Length > 0) {
        tirage.SetPlaques(arguments);
        if (arguments.Length > 6) tirage.Search = arguments[6];
    }
}

if (Json) {
    await tirage.ResolveAsync();
    WriteLine(JsonSerializer.Serialize(tirage.Data, JsonOptions()));
} else {
    WriteLine("\n");
    WriteLine("*** Le Compte est bon ***".Red());
    WriteLine();
    Write("Tirage:\t".Yellow());
    Write("Plaques: ".LightYellow());
    foreach (var plaque in tirage.Plaques) Write($"{plaque} ");
    Write("Recherche: ".LightYellow());
    Write(tirage.Search);

    WriteLine();
    var dt = DateTime.Now;
    var result = await tirage.ResolveAsync();
    var ts = DateTime.Now - dt;

    WriteLine();

    if (tirage.Status == CebStatus.Invalide) {
        WriteLine("Tirage invalide".Red());
    } else {
        var txtStatus = result == CebStatus.CompteEstBon ? result.Green() :
            result.Magenta();
        Write(txtStatus);
        if (result == CebStatus.CompteApproche)
            Write($": {tirage.Found}");

        Write($", {"nombre de solutions:".LightYellow()} {tirage.Solutions.Count}");
        WriteLine($", {"Durée du calcul:".LightYellow()} {ts.TotalMilliseconds / 1000:F3}");

        WriteLine();

        foreach (var (i, solution) in tirage.Solutions.Select((v, i) => (i, v))) {
            var count = $"{ i + 1,4}/{tirage.Count,4} ({solution.Rank}):".Cyan();
            WriteLine($"{txtStatus} {count} {solution}");
        }
    }

    WriteLine();
}

if (SaveToMongoDb && MongoServer != String.Empty)
    await SaveMongoDB();

if (Save == false)
    Environment.Exit(0);

if (fichier != null && Save == true) {
    switch (fichier.Extension) {
        case ".zip":
            SaveZip();
            break;

        default:
            SaveJson();
            break;
    }
}

void SaveJson() {
    using Stream stream = fichier.Create();
    JsonSaveStream(stream);
}

void SaveZip() {
    using var archive = ZipFile.Open(fichier.FullName, ZipArchiveMode.Update, System.Text.Encoding.UTF8);
    int num = new[] { 0 }.Concat(archive.Entries
       .Select(p => int.TryParse(p.Name[..p.Name.LastIndexOf('.')], out var result) ? result : 0))
       .Max();
    SaveStream(archive, $"{++num:000000}.json", JsonSaveStream);
    SaveStream(archive, $"{++num:000000}.xml", XmlSaveStream);
}

void SaveStream(ZipArchive archive, string nom, Action<Stream> action) {
    var stream = archive.CreateEntry(nom, CompressionLevel.SmallestSize).Open();
    action(stream);
    stream.Close();
}

void JsonSaveStream(Stream stream) {
    JsonSerializer.Serialize<CebData>(stream, tirage.Data, JsonOptions());
}

void XmlSaveStream(Stream stream) {
    XmlSerializer mySerializer = new(typeof(CebData));
    try {
        mySerializer.Serialize(stream, tirage.Data);
    } catch (SerializationException) {
        WriteLine("Erreur serialisation");
        throw;
    };
}

JsonSerializerOptions JsonOptions() => new() {
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    Converters ={
                new JsonStringEnumConverter()
            },
    WriteIndented = true
};

async Task SaveMongoDB() {
    try {
        ConventionRegistry.Register("EnumStringConvention",
           new ConventionPack {
                new EnumRepresentationConvention(BsonType.String)
           },
           t => true);
        var clientSettings = MongoClientSettings.FromConnectionString(MongoServer);
        clientSettings.LinqProvider = LinqProvider.V3;

        var cl = new MongoClient(clientSettings)
            .GetDatabase("ceb")
            .GetCollection<BsonDocument>("comptes");

        await cl.InsertOneAsync(
            new BsonDocument(new Dictionary<string, object> {
                { "_id",  new  { lang="c#", domain=Environment.GetEnvironmentVariable("USERDOMAIN"), date= DateTime.UtcNow }.ToBsonDocument() } })
            .AddRange(tirage.Data.ToBsonDocument()));
    } catch (Exception e) {
        WriteLine(e);
    }
}

internal static class Colorizer {

    public static string ColorForeground(this object texte, AnsiControlCode foreground) => $"{foreground}{texte}{Ansi.Color.Foreground.Default}";

    public static string Red(this object texte) => texte.ColorForeground(Ansi.Color.Foreground.Red);

    public static string LightYellow(this object texte) => texte.ColorForeground(Ansi.Color.Foreground.LightYellow);

    public static string Cyan(this object texte) => texte.ColorForeground(Ansi.Color.Foreground.Cyan);

    public static string Green(this object texte) => texte.ColorForeground(Ansi.Color.Foreground.Green);

    public static string Magenta(this object texte) => texte.ColorForeground(Ansi.Color.Foreground.Magenta);

    public static string Yellow(this object texte) => texte.ColorForeground(Ansi.Color.Foreground.Yellow);

    public static string Blue(this object texte) => texte.ColorForeground(Ansi.Color.Foreground.Blue);
}