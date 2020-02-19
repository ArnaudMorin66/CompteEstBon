using CompteEstBon;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Linq;
using static System.Console;

namespace CsCeb {
    internal class Program {
        private static void Main(string[] args) {
            var tirage = new CebTirage();
            var parser = new CommandLineApplication(false);
            var argPlaques = parser.Argument("plaques", "liste des plaques", multipleValues: true);
            var searchOption = parser.Option("-s | --search", "valeur à rechercher", CommandOptionType.SingleValue);
            var waitOption = parser.Option("-w | --wait", "Attente fin calcul", CommandOptionType.NoValue);
            parser.HelpOption("-h | --help");
            parser.OnExecute(() => {
                if (searchOption.HasValue()) {
                    if (int.TryParse(searchOption.Value(), out int search)) {
                        tirage.Search = search;
                    }
                    else {
                        return -2;
                    }
                }
                var ix = 0;
                foreach (var value in argPlaques.Values) {
                    if (int.TryParse(value, out int plaque)) {
                        tirage.Plaques[ix++].Value = plaque;
                    }
                    else {
                        return -1;
                    }
                }
                return 0;
            });
            parser.Execute(args);
            var (ts, result) = EvalTime(tirage.Resolve);
            WriteLine("** Le Compte est bon **");
            Write("Tirage:\t");
            WriteLine($"Recherche: {result.Search}");
            Write("Plaques: ");
            foreach (var plaque in result.Plaques) {
                Write($"{plaque} ");
            }
            WriteLine();
            WriteLine();
            WriteLine($"Durée du calcul: {ts.TotalMilliseconds / 1000}");

            if (tirage.Status == CebStatus.Erreur) {
                WriteLine("Tirage invalide");
            }
            else {
                Write(result.Status == CebStatus.CompteEstBon
                    ? "Compte est bon"
                    : $"Compte approché: {result.Found} ");

                WriteLine($", nombre de solutions {result.Solutions.Count()}");
                WriteLine();
                foreach (var solution in result.Solutions) {
                    WriteLine(solution.ToString());
                }
            }
            WriteLine();
            WriteLine("** fin calcul **");
            if (waitOption.HasValue())
                ReadLine();
        }

        private static (TimeSpan t, T) EvalTime<T>(Func<T> func) {
            var dt = DateTime.Now;
            var res = func();
            return (DateTime.Now - dt, res);
        }
    }
}