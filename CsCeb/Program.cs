﻿//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;

using arnaud.morin.outils;

using Microsoft.Extensions.Configuration;

using static System.Console;


bool json = false;
bool jsonx = false;
bool save = false;
bool afficher = false;
bool wait = false;
string configurationFile =
    @$"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Ceb\config.json";
string mongoServer = string.Empty;
bool saveToMongoDb = false;
List<FileInfo> exports = new();
FileInfo zipfile = null;
CebTirage tirage = new();
if (File.Exists(configurationFile)) {
    IConfigurationRoot configs = new ConfigurationBuilder().AddJsonFile(configurationFile).Build();
    foreach (IConfigurationSection config in configs.GetChildren())
        switch (config.Path.ToUpper()) {
            case "SAVE":
                save = bool.TryParse(config.Value, out bool v) && v;
                break;
            case "MONGODB":
                saveToMongoDb = bool.TryParse(config.Value, out bool wv) && wv;
                break;
            case "MONGODBSERVER":
                mongoServer = config.Value;
                break;
            case "ZIPFILE":
                try {
                    zipfile = new FileInfo(config.Value);
                } catch {
                    // ignored
                }

                break;
        }
}


RootCommand rootCommand = new("Compte Est Bon") {
    new Option<int>(new[] { "--trouve", "-t" }, "Nombre à chercher (entre 100 et 999)"),
    new Option<List<int>>(new[] { "--plaques", "-p" }, "Liste des plaques (6)") {
        AllowMultipleArgumentsPerToken = true
    },
    new Option<bool>(new[] { "--json", "-j" }, "Export au format JSON"),
    new Option<bool>(new[] { "--jsonx", "-J" }, "Export au format JSON et quitte"),
    new Option<bool>(new[] { "--sauvegarde", "-s" }, "Sauvegarder le Compte"),
    new Option<bool>(new[] { "--mongodb", "-m" }, "Sauvegarder le Compte dans MongoDB"),
    new Option<string>(new[] { "--serveur", "-S" }, "Nom du serveur MongoDB"),
    new Option<List<FileInfo>>(new[] { "--exports", "-x", "-f" }, "Exporter vers excel, word, json, xml, zip...") {
        AllowMultipleArgumentsPerToken = true
    },
    new Option<bool>(new[] { "--afficher", "-a" }, "Afficher les fichiers exportés"),
    new Option<bool>(new[] { "--wait", "-w" }, "Attendre touche"),
    new Argument<List<int>>("arguments", "Plaques (6) et nombre à trouver (entre 100 et 999)")
};


try {
    rootCommand.SetHandler(Handler);
    rootCommand.Invoke(args);
} catch (Exception ex) {
    Abort(ex);
}

if (wait) {
    Write(@"Appuyez sur une touche pour terminer...");
    ReadKey();
}

return;

void Run() {
    if (!jsonx) {
        WriteLine("*** LE COMPTE EST BON ***".Center(WindowWidth).Red());
        WriteLine();
    }

    tirage.Resolve();
    if (json) {
        WriteLine(tirage.WriteJson());
        if (jsonx)
            Environment.Exit(0);
    }

    WriteLine(
        $@"{"Plaques: ".LightYellow()}{string.Join(',', tirage.Plaques.Select(p => p.Value))}{"\t Recherche: ".LightYellow()}{tirage.Search}");
    WriteLine();

    if (tirage.Status == CebStatus.Invalide)
        throw new ArgumentException("Tirage  invalide");

    WriteLine(
        tirage.Status == CebStatus.CompteEstBon
            ? $"{Ansi.Color.Foreground.Green.EscapeSequence}Compte est bon{Ansi.Color.Foreground.Default.EscapeSequence}"
            : $"{Ansi.Color.Foreground.Magenta.EscapeSequence}Compte approché:{Ansi.Color.Foreground.Default.EscapeSequence} {tirage.Found}");
    WriteLine();

    WriteLine(
        $@"{"Nombre de solutions:".LightYellow()} {tirage.Count}{(tirage.Status == CebStatus.CompteApproche ?
            $@", {"écart:".LightYellow()} {tirage.Ecart}" : string.Empty)}, {"durée du calcul:".LightYellow()} {tirage.Duree.TotalSeconds:F3} s");

    WriteLine();

    foreach ((CebBase solution, int i) in tirage.Solutions!.Indexed()) {
        string count = $"{i + 1:0000}".ControlCode(
            tirage.Status == CebStatus.CompteEstBon ? Ansi.Color.Foreground.Green : Ansi.Color.Foreground.Magenta);
        // ReSharper disable once LocalizableElement
        WriteLine($"{count} ({solution.Rank}) =>\t{solution.LightYellow()}");
    }

    WriteLine();

    if (save) {
        if (zipfile != null && exports.Any(p => p.Extension == ".zip"))
            exports.Add(zipfile);
        ExportOffice.RegisterLicense(Resources.sflicence);

        tirage.SerializeFichiers(exports);
        if (saveToMongoDb && mongoServer != string.Empty)
            tirage.SerializeMongo(mongoServer);

        if (afficher)
            foreach (FileInfo export in exports)
                export.FullName.OpenDocument();
    }
}

void Abort(Exception ex, int retour = -1) {
    SetOut(Error);
    Write($@"{Ansi.Color.Foreground.White.EscapeSequence}{Ansi.Color.Background.Red.EscapeSequence}");
    WriteLine(@"+-----------------------------+".Center(WindowWidth));
    WriteLine(@"|          Erreur             |".Center(WindowWidth));
    WriteLine(@"+-----------------------------+".Center(WindowWidth));
    Write($@"{Ansi.Color.Foreground.Default.EscapeSequence}{Ansi.Color.Background.Default.EscapeSequence}");
    WriteLine($@"{ex.GetType()}: {ex.Message.Red()}".Center(WindowWidth));
    WriteLine();
    WriteLine($@"{"AIDE".ControlCode(Ansi.Text.UnderlinedOn, Ansi.Text.UnderlinedOff)} :");
    WriteLine();

    rootCommand.Invoke("-h");
    Environment.Exit(retour);
}

void Handler(InvocationContext context) {
    ParseResult parseResult = context.ParseResult;
    if (parseResult.UnparsedTokens.Count > 0)
        Abort(new ArgumentException($"Option(s) {string.Join(",", parseResult.UnparsedTokens)} invalide(s)"), -2);
    try {
        foreach (Option option in rootCommand.Options) {
            if (parseResult.FindResultFor(option) is not { } optionResult)
                continue;
            switch (option.Name.ToLower()) {
                case "trouve":
                    tirage.Search = optionResult.GetValueOrDefault<int>();
                    break;

                case "plaques":
                    tirage.SetPlaques(optionResult.GetValueOrDefault<List<int>>());
                    break;

                case "json":
                    json = optionResult.GetValueOrDefault<bool>();
                    jsonx = false;
                    break;

                case "jsonx":
                    json = optionResult.GetValueOrDefault<bool>();
                    jsonx = true;
                    break;

                case "exports":
                    exports.AddRange(optionResult.GetValueOrDefault<List<FileInfo>>());
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

                case "afficher":
                    afficher = optionResult.GetValueOrDefault<bool>();
                    break;
                case "wait":
                    wait = optionResult.GetValueOrDefault<bool>();
                    break;
            }
        }

        foreach (Argument argument in rootCommand.Arguments) {
            if (parseResult.FindResultFor(argument) is not { } argumentResult)
                continue;
            List<int> arguments = argumentResult.GetValueOrDefault<List<int>>();
            if (arguments.Count > 0) {
                if (arguments[0] > 100) {
                    tirage.Search = arguments[0];
                    arguments.RemoveAt(0);
                } else if (arguments.Count >= 7 && arguments[6] >= 100) {
                    tirage.Search = arguments[6];
                    arguments.RemoveAt(6);
                }

                if (arguments.Count > 0)
                    tirage.SetPlaques(arguments);
            }
        }

        Run();
    } catch (Exception e) {
        Abort(e);
    }
}