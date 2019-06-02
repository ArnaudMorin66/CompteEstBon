using System;
using System.Diagnostics;
using System.Linq;
using CompteEstBon;
using Microsoft.Extensions.CommandLineUtils;

namespace cscoreCeb {
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

                foreach (var (value,i) in argPlaques.Values.Select((value, i)=> (value, i))) {
                    if (int.TryParse(value, out int plaque)) {
                        tirage.Plaques[i].Value2 = plaque;
                    }
                    else {
                        return -1;
                    }
                }
                return 0;
            });
            parser.Execute(args);
            if(parser.IsShowingInformation)
                return;

            var ts = ElapsedTime(tirage.Resolve);
            Console.WriteLine("** Le Compte est bon **");
            Console.Write("Tirage:\t");
            Console.WriteLine($"Recherche: {tirage.Search}");
            Console.Write("Plaques: ");
            foreach (var plaque in tirage.Plaques) {
                Console.Write($"{plaque.Value} ");
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Durée du calcul: {ts.TotalMilliseconds / 1000}");

            if (tirage.Status == CebStatus.Erreur) {
                Console.WriteLine("Tirage invalide");
            }
            else {
                Console.Write(tirage.Status == CebStatus.CompteEstBon
                    ? "Compte est bon"
                    : $"Compte approché: {tirage.Found} ");

                Console.WriteLine($", nombre de solutions {tirage.Solutions.Count}");
                Console.WriteLine();
                foreach (var (solution, i) in tirage.Solutions.Select((elt, i) => (elt,i))) {
                    Console.WriteLine($"{i:D4}: {solution.Detail}");
                }
            }
            Console.WriteLine();
            Console.WriteLine("** fin calcul **");
            if (waitOption.HasValue())
                Console.ReadLine();
        }

        public static TimeSpan ElapsedTime(Func<CebStatus> action) {
            var watch = Stopwatch.StartNew();
            action();
            watch.Stop();
            return watch.Elapsed;
        }
    }

}