using CompteEstBon;
using System.CommandLine;
using System.CommandLine.DragonFruit;
using System;
using System.Linq;
using static System.Console;
using Microsoft.VisualBasic.FileIO;

namespace CsCeb {
   
    internal class Program {
        private static void Main(  int? search=null , int[] plaques = null, bool wait = false) {
            var tirage = new CebTirage();
            tirage.Search = search ?? tirage.Search;
          
            if (plaques != null) {
                tirage.SetPlaques(plaques);
            }
           
       
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