//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using CompteEstBon;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.Rendering;
using static System.Console;


var Json = false;
var Jsonx = false;
var Save = false;
var ConfigurationFile =
    @$"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Ceb\config.json";
var MongoServer = string.Empty;
var SaveToMongoDb = false;
List<FileInfo> fichiers = new();
var sflicence = CompteEstBon.Properties.Resources.sflicence;


var tirage = new CebTirage();
WriteLine('\n');
WriteLine("*** Le Compte est bon ***".Red());
WriteLine();

ConfigurationBuilder builder = new();
if(File.Exists(ConfigurationFile)) {
    builder.AddJsonFile(ConfigurationFile);
    var jbuild = builder.Build();

    foreach (var child in jbuild.GetChildren()) {
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
                var elt = child.GetChildren().FirstOrDefault(it => it.Key == Environment.OSVersion.Platform.ToString());
                if (elt != null)
                    fichiers.Add(new FileInfo(elt["ZipFile"]!)); 
                break;
            case "SFLICENCE":
                sflicence = child.Value;
                break;
        }
    }
}


var rootCommand = new RootCommand("Compte Est Bon")
{
    new Option<int>(new[] { "--trouve", "-t" }, "Nombre à chercher"),
    new Option<int[]>(new[] { "--plaques", "-p" }, "Liste des plaques") { AllowMultipleArgumentsPerToken = true },
    new Option<bool>(new[] { "--json", "-j" }, "Export au format JSON"),
    new Option<bool>(new[] { "--jsonx", "-J" }, "Export au format JSON et quitte"),
    new Option<bool>(new[] { "--sauvegarde", "-s" }, "Sauvegarder le Compte"),
    new Option<bool>(new[] { "--mongodb", "-m" }, "Sauvegarder le Compte dans MongoDB"),
    new Option<string>(new[] { "--serveur", "-S" }, "Nom du serveur MongoDB"),
    new Option<FileInfo[]>(new[] { "--fichiers", "-f" }, "Liste des fichiers"){ AllowMultipleArgumentsPerToken = true },
    new Argument<int[]>("arguments", "Plaques et nombre à trouver")
};
try {
    rootCommand.SetHandler(
        async context => {
            var prs = context.ParseResult;
            try {
                foreach(var option in rootCommand.Options)
                    if(prs.FindResultFor(option) is { } optionResult)
                        switch(option.Name.ToLower()) {
                            case "trouve":
                                tirage.Search = optionResult.GetValueOrDefault<int>();
                                break;

                            case "plaques":
                                tirage.SetPlaques(optionResult.GetValueOrDefault<int[]>());
                                break;

                            case "json":
                                (Json, Jsonx) = (optionResult.GetValueOrDefault<bool>(), false);
                                break;

                            case "fichiers":
                                fichiers.Clear();
                                fichiers.AddRange(optionResult.GetValueOrDefault<FileInfo[]>());
                                Save = true;
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
                            case "jsonx":
                                (Json, Jsonx) = (optionResult.GetValueOrDefault<bool>(), true);
                                break;

                            default:
                                throw new Exception($"Argument {option.Name} invalide");
                        }


                if(prs.FindResultFor(rootCommand.Arguments[0]) is { } argumentResult) {
                    var arguments = argumentResult.GetValueOrDefault<int[]>();

                    // if (arguments.Length > 7) throw new Exception("Nombre d'arguments invalide");

                    if(arguments[0] > 100) {
                        tirage.Search = arguments[0];
                        arguments = arguments[1..];
                    } else if(arguments.Length >= 7 && arguments[6] > 100) {
                        tirage.Search = arguments[6];
                        arguments = arguments[..6];
                    }
                    if(arguments.Length > 0)
                        tirage.SetPlaques(arguments);
                }


                await runAsync().ConfigureAwait(false);
            } catch(Exception e) {
                abort(e);
            }
        });

    await rootCommand.InvokeAsync(args).ConfigureAwait(false);
} catch(Exception ex) {
    abort(ex);
}

WriteLine();

void abort(Exception ex) {
    WriteLine();
    WriteLine(@"*********** Erreur ***********");
    WriteLine();
    WriteLine($@"Source : ${ex.Source}");
    WriteLine();
    WriteLine($@"{ex.Message.Red()}");
    WriteLine();
    WriteLine(@"*****************************");
    WriteLine();
    Environment.Exit(-1);
}

async Task runAsync() {
    await tirage.ResolveAsync().ConfigureAwait(false);
    if(Json) {
        tirage.WriteJson();
        if(Jsonx)
            Environment.Exit(0);
        WriteLine();
    }

    Write("Tirage:\t".Yellow());
    Write("Plaques: ".LightYellow());
    foreach(var plaque in tirage.Plaques)
        Write($@"{plaque} ");
    Write("Recherche: ".LightYellow());
    WriteLine(tirage.Search);
    WriteLine();

    if(tirage.Status == CebStatus.Invalide)
        throw new Exception("Tirage  invalide");


    var txtStatus = tirage.Status.ToString().ToUpper();
    txtStatus = tirage.Status == CebStatus.CompteEstBon ? txtStatus.Green() : txtStatus.Magenta();
    Write(txtStatus);
    if(tirage.Status == CebStatus.CompteApproche)
        Write($@": {tirage.Found}");

    Write($@", {"nombre de solutions:".LightYellow()} {tirage.Solutions!.Count}");
    WriteLine($@", {"Durée du calcul:".LightYellow()} {tirage.Duree:F3} s");

    WriteLine();

    foreach (var (i, solution) in tirage.Solutions.Select((v, i) => (i, v))) {
        var count = $"{i + 1:0000}".ColorForeground(
            tirage.Status == CebStatus.CompteEstBon ? Ansi.Color.Foreground.Green : Ansi.Color.Foreground.Magenta);
        WriteLine($@"{solution.Rank} - {count}: {solution.LightYellow()}");
    }

    WriteLine();

    if(Save) {
        SfCebOffice.RegisterLicense(sflicence);
        if (SaveToMongoDb && MongoServer != string.Empty)
            await tirage.SerializeTirageMongoAsync(MongoServer).ConfigureAwait(false);
        if(fichiers != null)
            tirage.SerializeFichiers(fichiers);
    }
}