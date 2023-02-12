//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using arnaud.morin.outils;
using CompteEstBon;
using CompteEstBon.Properties;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
using static System.Console;


var json = false;
var jsonx = false;
var save = false;
var afficher = false;
var wait = false;
var configurationFile = @$"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Ceb\config.json";
var mongoServer = string.Empty;
var saveToMongoDb = false;
var exports = new List<FileInfo>();
var sflicence = Resources.sflicence;
FileInfo zipfile = null;
var tirage = new CebTirage();
WriteLine('\n');
WriteLine("*** LE COMPTE EST BON ***".Center(WindowWidth).Red());

WriteLine();

ConfigurationBuilder builder = new();
if(File.Exists(configurationFile)) {
    builder.AddJsonFile(configurationFile);
    var jbuild = builder.Build();

    foreach(var child in jbuild.GetChildren())
        switch(child.Path.ToUpper()) {
            case "SAVE":
                save = bool.TryParse(child.Value, out var v) && v;
                break;
            case "MONGODB":
                saveToMongoDb = bool.TryParse(child.Value, out var wv) && wv;
                break;
            case "MONGODBSERVER":
                mongoServer = child.Value;
                break;
            case "ZIPFILE":
                zipfile = child.Value.FileInfo();
                break;
            case "SFLICENCE":
                sflicence = child.Value;
                break;
        }
}


var rootCommand = new RootCommand("Compte Est Bon")
{
    new Option<int>(new[] { "--trouve", "-t" }, "Nombre à chercher (entre 100 et 999)"),
    new Option<List<int>>(new[] { "--plaques", "-p" }, "Liste des plaques (6)")
    {
        AllowMultipleArgumentsPerToken = true
    },
    new Option<bool>(new[] { "--json", "-j" }, "Export au format JSON"),
    new Option<bool>(new[] { "--jsonx", "-J" }, "Export au format JSON et quitte"),
    new Option<bool>(new[] { "--sauvegarde", "-s" }, "Sauvegarder le Compte"),
    new Option<bool>(new[] { "--mongodb", "-m" }, "Sauvegarder le Compte dans MongoDB"),
    new Option<string>(new[] { "--serveur", "-S" }, "Nom du serveur MongoDB"),
    new Option<List<string>>(new[] { "--export", "-x", "-f" }, "Exporter resultats")
    {
        AllowMultipleArgumentsPerToken = true
    },
    new Option<bool>(new[] { "--afficher", "-a" }, "Afficher les fichiers exportés"),
    new Option<bool>(new[] { "--wait", "-w" }, "Attendre touche"),
    new Argument<List<int>>("arguments", "Plaques (6) et nombre à trouver (entre 100 et 999)")
};


void Handler(InvocationContext context) {
    var parseResult = context.ParseResult;
    if(parseResult.UnparsedTokens.Count > 0)
        Abort(new ArgumentException($"Option(s) {string.Join(",", parseResult.UnparsedTokens)} invalide(s)"), -2);

    try {
        foreach(var option in rootCommand.Options) {
            if(parseResult.FindResultFor(option) is not { } optionResult)
                continue;
            switch(option.Name.ToLower()) {
                case "trouve":
                    tirage.Search = optionResult.GetValueOrDefault<int>();
                    break;

                case "plaques":
                    tirage.SetPlaques(optionResult.GetValueOrDefault<List<int>>());
                    break;

                case "json":
                    (json, jsonx) = (optionResult.GetValueOrDefault<bool>(), false);
                    break;

                case "export":
                    exports.AddRange(optionResult.GetValueOrDefault<List<string>>().Select(CebStatic.FileInfo));
                    save = true;
                    break;

                case "sauvegarde":
                    save = optionResult.GetValueOrDefault<bool>();
                    break;

                case "mongodb":
                    saveToMongoDb = optionResult.GetValueOrDefault<bool>();
                    save = true;
                    break;

                case "serveur":
                    mongoServer = optionResult.GetValueOrDefault<string>();
                    break;

                case "jsonx":
                    (json, jsonx) = (optionResult.GetValueOrDefault<bool>(), true);
                    break;

                case "afficher":
                    afficher = optionResult.GetValueOrDefault<bool>();
                    break;
                case "wait":
                    wait = optionResult.GetValueOrDefault<bool>();
                    break;
            }
        }

        foreach(var argument in rootCommand.Arguments) {
            if(parseResult.FindResultFor(argument) is not { } argumentResult)
                continue;
            var arguments = argumentResult.GetValueOrDefault<List<int>>();
            if(arguments.Count <= 0)
                continue;
            if(arguments[0] > 100) {
                tirage.Search = arguments[0];
                arguments.RemoveAt(0);
            } else if(arguments.Count >= 7 && arguments[6] >= 100) {
                tirage.Search = arguments[6];
                arguments.RemoveAt(6);
            }

            if(arguments.Count > 0)
                tirage.SetPlaques(arguments);
        }
    } catch(Exception e) {
        Abort(e);
    }
}

async Task RunAsync() {
    tirage.Resolve();
    if(json) {
        tirage.WriteJson();
        if(jsonx)
            Environment.Exit(0);
    }

    var message = string.Empty;
    message += "Plaques: ".LightYellow();

    message += string.Join(',', tirage.Plaques.Select(p=> p.Value));
    message += "\t Recherche: ".LightYellow();
    message += $"{tirage.Search}";
    WriteLine(message);
    WriteLine();

    if(tirage.Status == CebStatus.Invalide)
        throw new ArgumentException("Tirage  invalide");

    message = tirage.Status == CebStatus.CompteEstBon ? "Compte est bon" : "Compte approché";
    message = tirage.Status == CebStatus.CompteEstBon ? message.Green() : message.Magenta();
    if(tirage.Status == CebStatus.CompteApproche)
        message += $@": {tirage.Found}";
    WriteLine(message);
    WriteLine();

    message = string.Empty;


    message += $@"{"Nombre de solutions:".LightYellow()} {tirage.Count}";
    if(tirage.Status == CebStatus.CompteApproche)
        message += $@", {"écart:".LightYellow()} {tirage.Diff}";
    message += $@", {"durée du calcul:".LightYellow()} {tirage.Duree.TotalSeconds:F3} s";
    WriteLine(message);

    WriteLine();

    foreach (var ( solution, i) in tirage.Solutions!.WithIndex()) {
        var count = $"{i + 1:0000}".ControlCode(
            tirage.Status == CebStatus.CompteEstBon ? Ansi.Color.Foreground.Green : Ansi.Color.Foreground.Magenta);
        WriteLine($@"{solution.Rank}: {count} => {solution.LightYellow()}");
    }

    WriteLine();

    if(save) {
        if(zipfile != null && exports.Any(p => p.Extension == ".zip"))
            exports.Add(zipfile);
        ExportOffice.RegisterLicense(sflicence);
        if(saveToMongoDb && mongoServer != string.Empty)
            await tirage.SerializeMongoAsync(mongoServer);
        tirage.SerializeFichiers(exports);

        if(afficher)
            foreach(var export in exports)
                Outils.OpenDocument(export.FullName);
    }
}


void Abort(Exception ex, int retour = -1) {
    WriteLine(Ansi.Color.Foreground.White.EscapeSequence + Ansi.Color.Background.Red.EscapeSequence);

    WriteLine(@"+-----------------------------+".Center(WindowWidth));
    WriteLine(@"|          Erreur             |".Center(WindowWidth));
    WriteLine(@"+-----------------------------+".Center(WindowWidth));
    WriteLine(Ansi.Color.Foreground.Default.EscapeSequence + Ansi.Color.Background.Default.EscapeSequence);
    WriteLine($@"{ex.GetType()}: {ex.Message.Red()}".Center(WindowWidth));
    WriteLine();
    WriteLine($@"{"AIDE".ControlCode(Ansi.Text.UnderlinedOn, Ansi.Text.UnderlinedOff)} :");
    WriteLine();
    rootCommand.Invoke("-h");
    Environment.Exit(retour);
}

try {
    rootCommand.SetHandler(Handler);
    rootCommand.Invoke(args);
    await RunAsync();
} catch(Exception ex) {
    Abort(ex);
}

WriteLine();
if(wait) {
    Write(@"Appuyez sur une touche pour terminer...");
    ReadKey();
}

#pragma warning disable CA1050
/// <summary>
///
/// </summary>
public static class CebStatic {
    /// <summary>
    ///
    /// </summary>
    public static readonly string TelechargementFolder =
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\Downloads";
    /// <summary>
    ///
    /// </summary>
    /// <param name="fichier"></param>
    /// <returns></returns>
    public static FileInfo FileInfo(this string fichier) => new(
        fichier.IndexOfAny(new[] { '\\', '/' }) < 0 ? $"{TelechargementFolder}\\{fichier}" : fichier);
}
#pragma warning restore CA1050