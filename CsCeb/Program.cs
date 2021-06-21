using System;
using System.Linq;
using CompteEstBon;
using static System.Console;

namespace CsCeb {
    /// <summary>
    /// </summary>
    public class Program {
        static void Main(int? search = null, int[] plaques = null, bool wait = false) {
            var tirage = new CebTirage();
            tirage.Search = search ?? tirage.Search;

            if (plaques != null) tirage.SetPlaques(plaques);


            var (ts, result) = EvalTime(tirage.Resolve);
            WriteLine("** Le Compte est bon **");
            Write("Tirage:\t");
            WriteLine($"Recherche: {tirage.Search}");
            Write("Plaques: ");
            foreach (var plaque in tirage.Plaques) Write($"{plaque} ");
            WriteLine();
            WriteLine();
            WriteLine($"Durée du calcul: {ts.TotalMilliseconds / 1000}");

            if (tirage.Status == CebStatus.Erreur) {
                WriteLine("Tirage invalide");
            }
            else {
                Write(result == CebStatus.CompteEstBon
                    ? "Compte est bon"
                    : $"Compte approché: {tirage.Found} ");

                WriteLine($", nombre de solutions {tirage.Solutions.Count()}");
                WriteLine();
                foreach (var solution in tirage.Solutions) WriteLine(solution.ToString());
            }

            WriteLine();
            WriteLine("** fin calcul **");
            if (wait)
                ReadLine();
        }

        private static (TimeSpan t, T) EvalTime<T>(Func<T> func) {
            var dt = DateTime.Now;
            var res = func();
            return (DateTime.Now - dt, res);
        }
    }
}