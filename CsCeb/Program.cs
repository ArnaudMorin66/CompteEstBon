//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.CommandLine;
using System.CommandLine.Rendering;
using CompteEstBon;
using CompteEstBon.Properties;
using Microsoft.Extensions.Configuration;
using static System.Console;


var Json = false;
var Jsonx = false;
var Save = false;
var Afficher = false;

var ConfigurationFile = @$"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Ceb\config.json";
var TelechargementFolder = $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\Downloads";

var MongoServer = string.Empty;
var SaveToMongoDb = false;
var exports = new List<FileInfo>();
var sflicence = Resources.sflicence;
FileInfo zipfile = null;
var tirage = new CebTirage();

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
            case "ZIPFILE":
                zipfile = FichierToFileInfo(child.Value!);
                break;
            case "SFLICENCE":
                sflicence = child.Value;
                break;
        }
}


var rootCommand = new RootCommand("Compte Est Bon") {
    new Option<int>(new[] { "--trouve", "-t" }, "Nombre à chercher (entre 100 et 999)"),
    new Option<List<int>>(new[] { "--plaques", "-p" }, "Liste des plaques (6)")
        { AllowMultipleArgumentsPerToken = true },
    new Option<bool>(new[] { "--json", "-j" }, "Export au format JSON"),
    new Option<bool>(new[] { "--jsonx", "-J" }, "Export au format JSON et quitte"),
    new Option<bool>(new[] { "--sauvegarde", "-s" }, "Sauvegarder le Compte"),
    new Option<bool>(new[] { "--mongodb", "-m" }, "Sauvegarder le Compte dans MongoDB"),
    new Option<string>(new[] { "--serveur", "-S" }, "Nom du serveur MongoDB"),
    new Option<List<string>>(new[] { "--export", "-x", "-f" }, "Exporter resultats")
        { AllowMultipleArgumentsPerToken = true },
    new Option<bool>(new[] { "--afficher", "-a" }, "Afficher les fichiers exportés"),
    new Argument<List<int>>("arguments", "Plaques (6) et nombre à trouver (entre 100 et 999)")
};


try {
    rootCommand.SetHandler(
        async context => {
            var prs = context.ParseResult;
            if (prs.UnparsedTokens.Count > 0) {
                abort(new ArgumentException($"Option(s) {string.Join(",", prs.UnparsedTokens)} invalide(s)"), -2);
            }

            try {
                foreach (var option in rootCommand.Options) {
                    if (prs.FindResultFor(option) is not { } optionResult) 
                        continue;
                    switch (option.Name.ToLower()) {
                        case "trouve":
                            tirage.Search = optionResult.GetValueOrDefault<int>();
                            break;

                        case "plaques":
                            tirage.SetPlaques(optionResult.GetValueOrDefault<List<int>>());
                            break;

                        case "json":
                            (Json, Jsonx) = (optionResult.GetValueOrDefault<bool>(), false);
                            break;

                        case "export":
                            exports.AddRange(optionResult.GetValueOrDefault<List<string>>()
                                .Select(FichierToFileInfo));
                            Save = true;
                            break;

                        case "sauvegarde":
                            Save = optionResult.GetValueOrDefault<bool>();
                            break;

                        case "mongodb":
                            SaveToMongoDb = optionResult.GetValueOrDefault<bool>();
                            Save = true;
                            break;

                        case "serveur":
                            MongoServer = optionResult.GetValueOrDefault<string>();
                            break;

                        case "jsonx":
                            (Json, Jsonx) = (optionResult.GetValueOrDefault<bool>(), true);
                            break;

                        case "afficher":
                            Afficher = optionResult.GetValueOrDefault<bool>();
                            break;

                    }
                }

                foreach (var argument in rootCommand.Arguments) {
                    if (prs.FindResultFor(argument) is not { } argumentResult) continue;
                    var arguments = argumentResult.GetValueOrDefault<List<int>>();
                    if (arguments.Count <= 0) continue;
                    if (arguments[0] > 100) {
                        tirage.Search = arguments[0];
                        arguments.RemoveAt(0);
                    }
                    else if (arguments.Count >= 7 && arguments[6] >= 100) {
                        tirage.Search = arguments[6];
                        arguments.RemoveAt(6);
                    }

                    if (arguments.Count > 0)
                        tirage.SetPlaques(arguments);
                }

                await runAsync();
            }
            catch (Exception e) {
                abort(e);
            }
        });

    await rootCommand.InvokeAsync(args);
   
}
catch (Exception ex) {
    abort(ex);
}

WriteLine();

void abort(Exception ex, int retour=-1) {
    WriteLine();
    WriteLine(@"+-----------------------------+");
    WriteLine(@"|          Erreur             |");
    WriteLine(@"+-----------------------------+");
    WriteLine();
    WriteLine($@"{ex.Source?? "ceb"}, {ex.GetType()} => {ex.Message.Red()}");
    WriteLine();
    WriteLine($@"{"AIDE".TextControlCode(Ansi.Text.UnderlinedOn, Ansi.Text.UnderlinedOff)} :");
    WriteLine();
    rootCommand.Invoke("-h");
    Environment.Exit(retour);
}

async Task runAsync() {
    await tirage.ResolveAsync();
    if (Json) {
        tirage.WriteJson();
        if (Jsonx)
            Environment.Exit(0);
        WriteLine();
    }

    Write("Tirage:\t".Yellow());
    Write("Plaques: ".LightYellow());

    foreach (var plaque in tirage.Plaques)
        Write($@"{plaque} ");

    Write(", Recherche: ".LightYellow());
    WriteLine(tirage.Search);
    WriteLine();

    if (tirage.Status == CebStatus.Invalide)
        throw new ArgumentException("Tirage  invalide");

    var txtStatus = tirage.Status.ToString().ToUpper();
    txtStatus = tirage.Status == CebStatus.CompteEstBon ? txtStatus.Green() : txtStatus.Magenta();
    Write(txtStatus);
    if (tirage.Status == CebStatus.CompteApproche)
        Write($@": {tirage.Found}");

    Write($@", {"nombre de solutions:".LightYellow()} {tirage.Solutions!.Count}");
    WriteLine($@", {"Durée du calcul:".LightYellow()} {tirage.Duree:F3} s");

    WriteLine();

    foreach (var (i, solution) in tirage.Solutions.Select((v, i) => (i, v))) {
        var count = $"{i + 1:0000}".TextControlCode(
            tirage.Status == CebStatus.CompteEstBon ? Ansi.Color.Foreground.Green : Ansi.Color.Foreground.Magenta);
        WriteLine($@"{solution.Rank}: {count} => {solution.LightYellow()}");
    }

    WriteLine();

    if (Save) {
        if (zipfile != null && exports.Any(p => p.Extension == ".zip")) exports.Add(zipfile);
        ExportOffice.RegisterLicense(sflicence);
        if (SaveToMongoDb && MongoServer != string.Empty)
            await tirage.SerializeTirageMongoAsync(MongoServer);
        tirage.SerializeFichiers(exports);

        if (Afficher)
            foreach (var export in exports)
                Utilitaires.OpenDocument(export.FullName);
    }
}

FileInfo FichierToFileInfo(string fichier) =>
    new(fichier.IndexOfAny(new[] { '\\', '/' }) < 0 ? $"{TelechargementFolder}\\{fichier}" : fichier);