using CompteEstBon;
using Microsoft.Extensions.CommandLineUtils;
using System;
using static System.Console;

namespace CsCeb
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var tirage = new CebTirage();
            var parser = new CommandLineApplication(false);
            var argPlaques = parser.Argument("plaques", "liste des plaques", multipleValues: true);
            var searchOption = parser.Option("-s | --search", "valeur à rechercher", CommandOptionType.SingleValue);
            var waitOption = parser.Option("-w | --wait", "Attente fin calcul", CommandOptionType.NoValue);
            parser.HelpOption("-h | --help");
            parser.OnExecute(() =>
            {
                if (searchOption.HasValue())
                {
                    if (int.TryParse(searchOption.Value(), out int search))
                    {
                        tirage.Search = search;
                    }
                    else
                    {
                        return -2;
                    }
                }
                var ix = 0;
                foreach (var value in argPlaques.Values)
                {
                    if (int.TryParse(value, out int plaque))
                    {
                        tirage.Plaques[ix++].Value = plaque;
                    }
                    else
                    {
                        return -1;
                    }
                }
                return 0;
            });
            parser.Execute(args);
            var ts = EvalTime(tirage.Resolve, out CebStatus result);
            WriteLine("** Le Compte est bon **");
            Write("Tirage:\t");
            WriteLine($"Recherche: {tirage.Search}");
            Write("Plaques: ");
            foreach (var plaque in tirage.Plaques)
            {
                Write($"{plaque.Value} ");
            }
            WriteLine();
            WriteLine();
            WriteLine($"Durée du calcul: {ts.TotalMilliseconds / 1000}");

            if (result == CebStatus.Erreur)
            {
                WriteLine("Tirage invalide");
            }
            else
            {
                Write(result == CebStatus.CompteEstBon
                    ? "Compte est bon"
                    : $"Compte approché: {tirage.Found} ");

                WriteLine($", nombre de solutions {tirage.Solutions.Count}");
                WriteLine();
                foreach (var solution in tirage.Solutions)
                {
                    var ii = 0;
                    foreach (var operation in solution.Operations)
                    {
                        if (ii++ > 0)
                        {
                            Write(", ");
                        }
                        Write(operation);
                    }
                    WriteLine();
                }
            }
            WriteLine();
            WriteLine("** fin calcul **");
            if (waitOption.HasValue())
                ReadLine();
        }

        private static TimeSpan EvalTime(Func<CebStatus> action, out CebStatus status)
        {
            var dt = DateTime.Now;
            status = action();
            return DateTime.Now - dt;
        }
    }
}