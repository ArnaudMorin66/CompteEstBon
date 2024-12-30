//-----------------------------------------------------------------------
// <copyright file="Ceb.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using arnaud.morin.outils;

using Spectre.Console.Json;

using System.CommandLine;

using System.CommandLine.NamingConventionBinder;

using System.Diagnostics;

using static Spectre.Console.AnsiConsole;

Ceb.Factory.Run(args);

internal class Ceb {
    public CebParametres Param;
    private Ceb() => Param = CebParametres.Get;
    public static Ceb Factory { get; } = new();
    public string elapsedtime = "00:00:00.000";
    private void Solve() {
        var tirage = Param.Tirage;
        if (!Param.Jsonx) {
            DisplayHeader();
            DisplayTirageDetails(tirage);
        }

        var ligne = System.Console.CursorTop;

        var stopwatch = new Stopwatch();
        var timerChrono = new Timer(_ => {
            elapsedtime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff");
            Cursor.SetPosition(0, ligne);
            Write(new String(' ', System.Console.WindowWidth));
            Cursor.SetPosition(0, ligne);
            Write(Align.Center(new Markup($"[green]{elapsedtime}[/]")));
        }, null, 0, 100);
        stopwatch.Start();
        tirage.Solve();
        stopwatch.Stop();
        elapsedtime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff");
        timerChrono.Dispose();
        Cursor.SetPosition(0, ligne);
        if (Param.Jsonx) {
            WriteLine(tirage.WriteJson());
            Environment.Exit(0);
        }
        if (Param.Json) {
            DisplayJsonOutput(tirage);
        }
        ValidateTirage(tirage);
        DisplayResults(tirage);
        DisplaySolutions(tirage);
        if (Param.Exports is { Count: > 0 }) {
            ExportResults(tirage);
        }
    }
    private void DisplayHeader() {
        Write(
            new FigletText("COMPTE EST BON")
                .Centered()
                .Color(Color.Yellow));
        WriteLine();
        WriteLine();
    }
    private void DisplayTirageDetails(CebTirageBase tirage) {
        var res = new List<string> {
        "[yellow bold u]Recherche[/]:",
        tirage.Search.ToString(),
        "[yellow bold u]Plaques[/]:"
    };
        res.AddRange(tirage.Plaques.Select(p => p.ToString()));
        Write(Align.Center(new Panel(new Columns(res)).Border(BoxBorder.Square)));
        WriteLine();
        WriteLine();
    }
    private void DisplayJsonOutput(CebTirageBase tirage) {
        var jtext = new JsonText(tirage.WriteJson());
        Write(Align.Center(
            new Panel(jtext).Expand()
                .RoundedBorder()
                .BorderColor(Color.Yellow)));
    }
    private void ValidateTirage(CebTirageBase tirage) {
        if (tirage.Status == CebStatus.Invalide)
            throw new ArgumentException("Tirage invalide");
    }
    private void DisplayResults(CebTirageBase tirage) {
        Write(Align.Center(new Markup(
            tirage.Status == CebStatus.CompteEstBon
                ? "[green]Compte est bon[/]"
                : $"[orange1]Compte approché:[/] {tirage.Found}")));
        WriteLine();
        WriteLine();
        Write(Align.Center(new Markup(
            $@"[yellow]Nombre de solutions:[/] {tirage.Count}{(tirage.Status == CebStatus.CompteApproche ?
                $@", [yellow]écart:[/] {tirage.Ecart}" : string.Empty)}, [yellow]durée du calcul:[/] {elapsedtime}")));
        WriteLine();
        WriteLine();
    }
    private void DisplaySolutions(CebTirageBase tirage) {
        Background = Color.White;
        var couleur = tirage.Status == CebStatus.CompteEstBon ? Color.Green : Color.Orange1;
        var table = CreateSolutionsTable(couleur);
        var no = 0;
        Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .Start(ctx => {
                foreach (var solution in tirage.Solutions) {
                    var rw = new List<string> { $"[{couleur}]{++no}/{tirage.Count}[/]" };
                    rw.AddRange(solution.Operations);
                    table.AddRow(rw.ToArray());
                    ctx.Refresh();
                }
            });
        Background = Color.Default;
        WriteLine();
    }
    private Table CreateSolutionsTable(Color couleur) {
        return new Table()
            .Border(TableBorder.HeavyHead)
            .BorderColor(couleur)
            .AddColumns(
                new TableColumn($"[{couleur}]N°[/]").Alignment(Justify.Center),
                new TableColumn(Align.Center(new Markup($"[{couleur} b]Opération 1[/]"))),
                new TableColumn(Align.Center(new Markup($"[{couleur} b]Opération 2[/]"))),
                new TableColumn(Align.Center(new Markup($"[{couleur} b]Opération 3[/]"))),
                new TableColumn(Align.Center(new Markup($"[{couleur} b]Opération 4[/]"))),
                new TableColumn(Align.Center(new Markup($"[{couleur} b]Opération 5[/]"))));
    }
    private void ExportResults(CebTirageBase tirage) {
        ExportOffice.RegisterLicense(Resources.sflicence);
        tirage.SerializeFichiers(Param.Exports);
        if (Param.Display) {
            foreach (var export in Param.Exports) {
                export.FullName.OpenDocument();
            }
        }
    }


    public void Run(string[] args) {
        try {
            if (Param.Invoke(args) != 1) return;
            Solve();
            if (Param.Wait) {
                Write(@"Appuyez sur une touche pour terminer...");
                AnsiConsole.Console.Input.ReadKey(false);
            }
        } catch (Exception e) {
            Abort(e);
        }
    }


    private void Abort(Exception ex, int retour = -1) {
        Write(new FigletText("ERREUR")
            .Centered()
            .Color(Color.Red));
        Write(new FigletText(ex.Message)
            .Centered()
            .Color(Color.Red));
        WriteLine();

        MarkupLine("Aide".Yellow().Underline());
        WriteLine();
        Param.Root.Invoke("-h");
        Environment.Exit(retour);
    }
}

internal sealed record CebParametres {
    private static CebParametres? _parametres;
    private CebParametres() { }

    public static CebParametres Get => _parametres ??= new CebParametres();

    public bool Json { get; private set; }
    public bool Jsonx { get; private set; }
    public bool Display { get; private set; }
    public List<FileInfo> Exports { get; private set; } = [];

    public bool Wait { get; private set; }

    public CebTirageBase Tirage { get; } = new();

    public RootCommand Root { get; } = new("Compte Est Bon") {
        new Option<int>(["--search", "-s"], "Nombre à chercher (entre 100 et 999)"),
        new Option<List<int>>(["--plaques", "-p"], "Liste des plaques (6)") { AllowMultipleArgumentsPerToken = true },
        new Option<bool>(["--json", "-j"], "Export au format JSON"),
        new Option<bool>(["--jsonx", "-J"], () => false, "Export au format JSON et sortie"),
        new Option<List<FileInfo>>(["--exports", "-x", "-f"], "Exporter vers excel, word, json, xml, zip...") {
            AllowMultipleArgumentsPerToken = true
        },
        new Option<bool>(["--display", "-d"], "Afficher les fichiers exportés"),
        new Option<bool>(["--wait", "-w"], "Attendre touche"),
        new Argument<List<int>>("arguments", "Plaques (6) et nombre à trouver (entre 100 et 999)")
    };


    public int Invoke(string[] args) {
        Root.Handler = CommandHandler.Create(Handler);
        return Root.Invoke(args);
    }

    public int Invoke(ref string[] args) {
        Root.Handler = CommandHandler.Create(Handler);
        return Root.Invoke(args);
    }

    private int Handler(int search,
                        List<int> plaques,
                        bool json,
                        bool jsonx,
                        List<FileInfo> exports,
                        bool display,
                        bool wait,
                        List<int> arguments) {
        if (search != 0) Tirage.Search = search;
        if (plaques is { Count: > 0 }) Tirage.SetPlaques(plaques);
        Wait = wait;
        Json = json;
        Jsonx = jsonx;
        Display = display;
        if (exports is { Count: > 0 }) Exports = [.. exports];
        if (arguments is { Count: > 0 }) {
            if (arguments[0] is > 100) {
                Tirage.Search = arguments[0];
                arguments.RemoveAt(0);
            } else if (arguments[6] is > 100) {
                Tirage.Search = arguments[6];
                arguments.RemoveAt(6);
            }

            if (arguments.Count > 0)
                Tirage.SetPlaques(arguments);
        }

        return 1;
    }
}


internal static class CebColorizer {
    public static string Colorize(this string valeur, Color color) => $"[{color}]{valeur}[/]";

    public static string Red(this string valeur) => valeur.Colorize(Color.Red);
    public static string Blue(this string valeur) => valeur.Colorize(Color.Blue);
    public static string Yellow(this string valeur) => valeur.Colorize(Color.Yellow);
    public static string Rgb(this string valeur, byte r, byte g, byte b) => valeur.Colorize(new Color(r, g, b));
    public static string Bold(this string valeur) => $"[b]{valeur}[/]";
    public static string Underline(this string valeur) => $"[u]{valeur}[/]";
}