//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;

using arnaud.morin.outils;

using static Spectre.Console.AnsiConsole;


var json = false;
var jsonx = false;
var display = false;
var wait = false;
List<FileInfo> exports = [];
CebTirage tirage = new();

var optSearch = new Option<int?>(["--search", "-s"], "Nombre à chercher (entre 100 et 999)");

var optPlaques =
    new Option<List<int>?>(["--plaques", "-p"], "Liste des plaques (6)") { AllowMultipleArgumentsPerToken = true };

var optJson = new Option<bool?>(["--json", "-j"], "Export au format JSON");

var optJsonx = new Option<bool?>(["--jsonx", "-J"], "Export au format JSON et quitte");

var optExports =
    new Option<List<FileInfo>?>(["--exports", "-x", "-f"], "Exporter vers excel, word, json, xml, zip...") {
        AllowMultipleArgumentsPerToken = true
    };

var optDisplay = new Option<bool?>(["--display", "-d"], "Afficher les fichiers exportés");

var optWait = new Option<bool?>(["--wait", "-w"], "Attendre touche");

var argArguments = new Argument<List<int>?>("arguments", "Plaques (6) et nombre à trouver (entre 100 et 999)");


RootCommand commands = new("Compte Est Bon") {
    optSearch, optPlaques, optJson, optJsonx, optExports, optDisplay, optWait, argArguments
};

var parser = new CommandLineBuilder(commands)
    .UseDefaults()
    .UseParseDirective()
    .UseAnsiTerminalWhenAvailable()
    .UseHelp()
    .UseVersionOption()
    .UseSuggestDirective()
    .Build();

try {
    commands.SetHandler(async ctx => {
        var result = ctx.ParseResult;

        if (result.GetValueForOption(optSearch) is { } search) tirage.Search = search;

        if (result.GetValueForOption(optPlaques) is { Count: > 0 } plq) tirage.SetPlaques(plq);

        if (result.GetValueForOption(optJson) is { } jsonValue) json = jsonValue;

        if (result.GetValueForOption(optJsonx) is { } jsonxValue) jsonx = jsonxValue;

        if (result.GetValueForOption(optExports) is { Count: > 0 } fileInfos) exports.AddRange(fileInfos);

        if (result.GetValueForOption(optDisplay) is { } displaValue) display = displaValue;

        if (result.GetValueForOption(optWait) is { } waitValue) wait = waitValue;

        if (result.GetValueForArgument(argArguments) is { Count: > 0 } valeurs) {
            if (valeurs[0] > 100) {
                tirage.Search = valeurs[0];
                valeurs.RemoveAt(0);
            } else if (valeurs is [_, _, _, _, _, _, >= 100, ..]) {
                tirage.Search = valeurs[6];
                valeurs.RemoveAt(6);
            }

            if (valeurs.Count > 0)
                tirage.SetPlaques(valeurs);
        }

        await RunAsync();
    });
    await parser.InvokeAsync(args);

    //await commands.InvokeAsync(args);
} catch (Exception ex) {
    Abort(ex);
}

if (wait) {
    Write(@"Appuyez sur une touche pour terminer...");
    AnsiConsole.Console.Input.ReadKey(true);
}

return;

async Task RunAsync() {
    if (!jsonx) {
        Write(Align.Center(new Markup("[black on red bold u]*** LE COMPTE EST BON ***[/]")));
        WriteLine();
        WriteLine();
    }

    await tirage.ResolveAsync();

    if (json || jsonx) {
        WriteLine(tirage.WriteJson());
        if (jsonx) Environment.Exit(0);
    }

    var grid = new Grid();
    var res = new List<string> { "[yellow]Plaques:[/]" };
    res.AddRange(tirage.Plaques.Select(p => p.ToString()));
    res.Add("[yellow]Recherche:[/]");
    res.Add(tirage.Search.ToString());
    grid.AddColumns(9)
        .AddRow(res.ToArray());

    Write(Align.Center(grid));
    WriteLine();
    WriteLine();

    if (tirage.Status == CebStatus.Invalide) throw new ArgumentException("Tirage  invalide");

    Write(Align.Center(new Markup(
        tirage.Status == CebStatus.CompteEstBon
            ? "[green]CompteEstBon[/]"
            : $"[magenta]Compte approché[/]: {tirage.Found}")));
    WriteLine();
    WriteLine();

    Write(Align.Center(new Markup(
        $@"[yellow]Nombre de solutions:[/] {tirage.Count}{(tirage.Status == CebStatus.CompteApproche ?
            $@", [yellow]écart:[/] {tirage.Ecart}" : string.Empty)}, [yellow]durée du calcul:[/] {TimeSpan.FromSeconds(tirage.Duree.TotalSeconds)}")));

    WriteLine();
    WriteLine();
    var tab = new Table().Alignment(Justify.Center)
        .BorderColor(tirage.Status == CebStatus.CompteEstBon ? Color.Green : Color.Orange1)
        .AddColumns(new TableColumn("Numéro").Alignment(Justify.Center),
            new TableColumn("Opération 1").Alignment(Justify.Left),
            new TableColumn("Opération 2").Alignment(Justify.Left),
            new TableColumn("Opération 3").Alignment(Justify.Left),
            new TableColumn("Opération 4").Alignment(Justify.Left),
            new TableColumn("Opération 5").Alignment(Justify.Left));
    var no = 0;
    foreach (var solution in tirage.Solutions) {
        var rw = new List<string> { $"{++no}/{tirage.Count}" };
        rw.AddRange(solution.Operations);
        tab.AddRow(rw.ToArray());
    }

    Write(tab);
    WriteLine();

    if (exports.Count != 0) {
        ExportOffice.RegisterLicense(Resources.sflicence);
        tirage.SerializeFichiers(exports);

        if (display)
            foreach (var export in exports)
                export.FullName.OpenDocument();
    }
}

void Abort(Exception ex, int retour = -1) {
    // SetOut(Error);
    Foreground = Color.Red;
    Background = Color.White;
    // Write($@"{Ansi.Color.Foreground.Red.EscapeSequence}{Ansi.Color.Background.White.EscapeSequence}");
    Write(Align.Center(new Text("+-----------------------------+")));
    Write(Align.Center(new Text(@"|          Erreur             |")));
    Write(Align.Center(new Text(@"+-----------------------------+")));
    WriteLine();
    Foreground = Color.Default;
    Background = Color.Default;
    //Write($@"{Ansi.Color.Foreground.Default.EscapeSequence}{Ansi.Color.Background.Default.EscapeSequence}");
    Write(Align.Center(new Markup($@"[red]{ex.Message}")));
    WriteLine();
    MarkupLine(@"[u]AIDE[/]line()} :");

    WriteLine();
    parser.InvokeAsync("-h");
    Environment.Exit(retour);
}