//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

using arnaud.morin.outils;

using Spectre.Console.Json;

using static Spectre.Console.AnsiConsole;


List<FileInfo> listExports = [];
CebTirage tirage = new();

var optSearch = new Option<int?>(["--search", "-s"], "Nombre à chercher (entre 100 et 999)");

var optPlaques =
    new Option<List<int>?>(["--plaques", "-p"], "Liste des plaques (6)") { AllowMultipleArgumentsPerToken = true };

var optJson = new Option<bool>(["--json", "-j"], () => false, "Export au format JSON");

var optJsonx = new Option<bool>(["--jsonx", "-J"], () => false, "Export au format JSON et quitte");

var optExports =
    new Option<List<FileInfo>>(["--exports", "-x", "-f"], "Exporter vers excel, word, json, xml, zip...") {
        AllowMultipleArgumentsPerToken = true
    };

var optDisplay = new Option<bool>(["--display", "-d"], () => false, "Afficher les fichiers exportés");

var optWait = new Option<bool>(["--wait", "-w"], () => false, "Attendre touche");

var argArguments = new Argument<List<int>?>("arguments", "Plaques (6) et nombre à trouver (entre 100 et 999)");


RootCommand commands = new("Compte Est Bon") {
    optSearch, optPlaques, optJson, optJsonx, optExports, optDisplay, optWait, argArguments
};
// ReSharper disable InconsistentNaming
var Wait = false;
var Json = false;
var Jsonx = false;
var Display = false;
// ReSharper restore InconsistentNaming

try {
    commands.Handler = CommandHandler.Create<int?, List<int>, bool, bool, List<FileInfo>, bool, bool, List<int>?>(
        (search, plaques, json, jsonx, exports, display, wait, arguments) => {
            if (search is not null) tirage.Search = (int)search;
            if (plaques is { Count: > 0 }) tirage.SetPlaques(plaques);
            Wait = wait;
            Json = json;
            Jsonx = jsonx;
            Display = display;
            listExports.AddRange(exports);
            if (arguments is { Count: > 0 }) {
                if (arguments[0] is > 100) {
                    tirage.Search = arguments[0];
                    arguments.RemoveAt(0);
                } else if (arguments[6] is > 100) {
                    tirage.Search = arguments[6];
                    arguments.RemoveAt(6);
                }

                if (arguments.Count > 0)
                    tirage.SetPlaques(arguments);
            }

            return 1;
        });
    if (await commands.InvokeAsync(args) != 1) return;

    await RunAsync();
} catch (Exception e) {
    Abort(e);
}

if (Wait) {
    Write(@"Appuyez sur une touche pour terminer...");
    AnsiConsole.Console.Input.ReadKey(false);
}

return;

async Task RunAsync() {
    if (!Jsonx) {
        Write(
            new FigletText("COMPTE EST BON")
                .Centered()
                .Color(Color.Yellow));
        WriteLine();
        WriteLine();

        var res = new List<string> { "[yellow bold]Plaques:[/]" };
        res.AddRange(tirage.Plaques.Select(p => p.ToString()));
        res.Add("[yellow]Recherche:[/]");
        res.Add(tirage.Search.ToString());
        Write(Align.Center(new Panel(new Columns(res)).Border(BoxBorder.Square)));
        WriteLine();
        WriteLine();
    }


    await tirage.ResolveAsync();
    if (Jsonx) {
        WriteLine(tirage.WriteJson());
        Environment.Exit(0);
    }

    if (Json) {
        var jtext = new JsonText(tirage.WriteJson());
        Write(Align.Center(
            new Panel(jtext).Expand()
                .RoundedBorder()
                .BorderColor(Color.Yellow)));
    }

    if (tirage.Status == CebStatus.Invalide)
        throw new ArgumentException("Tirage  invalide");

    Write(Align.Center(new Markup(
        tirage.Status == CebStatus.CompteEstBon
            ? "[green]Compte est bon[/]"
            : $"[orange1]Compte approché:[/] {tirage.Found}")));
    WriteLine();
    WriteLine();

    Write(Align.Center(new Markup(
        $@"[yellow]Nombre de solutions:[/] {tirage.Count}{(tirage.Status == CebStatus.CompteApproche ?
            $@", [yellow]écart:[/] {tirage.Ecart}" : string.Empty)}, [yellow]durée du calcul:[/] {TimeSpan.FromSeconds(tirage.Duree.TotalSeconds)}")));

    WriteLine();
    WriteLine();
    var table = new Table().Alignment(Justify.Center)
        .Border(TableBorder.HeavyHead)
        //.ShowRowSeparators()
        .BorderColor(tirage.Status == CebStatus.CompteEstBon ? Color.Green : Color.Orange1)
        .AddColumns(
            new TableColumn("[red]N°[/]").Alignment(Justify.Center),
            new TableColumn("[yellow b]Opération 1[/]").Alignment(Justify.Left),
            new TableColumn("[yellow b]Opération 2[/]").Alignment(Justify.Left),
            new TableColumn("[yellow b]Opération 3[/]").Alignment(Justify.Left),
            new TableColumn("[yellow b]Opération 4[/]").Alignment(Justify.Left),
            new TableColumn("[yellow b]Opération 5[/]").Alignment(Justify.Left));
    var no = 0;
    await Live(table)
        .AutoClear(false)
        .Overflow(VerticalOverflow.Ellipsis)
        .StartAsync(ctx => {
            foreach (var solution in tirage.Solutions) {
                var rw = new List<string> { $"{++no}/{tirage.Count}" };
                rw.AddRange(solution.Operations);
                table.AddRow(rw.ToArray());
                ctx.Refresh();
            }

            return Task.CompletedTask;
        });
    //Write(tab);
    WriteLine();

    if (listExports.Count != 0) {
        ExportOffice.RegisterLicense(Resources.sflicence);
        tirage.SerializeFichiers(listExports);

        if (Display)
            foreach (var export in listExports)
                export.FullName.OpenDocument();
    }
}

void Abort(Exception ex, int retour = -1) {
    Write(new FigletText("ERREUR")
        .Centered()
        .Color(Color.Red));
    Write(new FigletText(ex.Message)
        .Centered()
        .Color(Color.Red));
    WriteLine();

    MarkupLine(@"[u yellow]AIDE[/] :");
    WriteLine();
    commands.Invoke("-h");
    Environment.Exit(retour);
}

