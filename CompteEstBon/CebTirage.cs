#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

#endregion using

namespace CompteEstBon {

    /// <summary>
    /// Gestion tirage Compte est bon
    /// </summary>
    [System.Runtime.InteropServices.Guid("EC9CF01C-34A0-414C-BF2A-D06C5A61503D")]
    public sealed class CebTirage {
        private int _search;

        public CebTirage() {
            for (var i = 0; i < 6; i++) {
                Plaques[i] = new CebPlaque(0);
            }
            // Capure des changements de valeur des plaques
            CebPlaque.ValueEvent += () => { if (EventActif) { Clear(); } };
            Random();
        }

        /// <summary>
        /// Constructeur Tirage du Compte est bon
        /// </summary>
        /// <param name="search">
        /// </param>
        /// <param name="plaques">
        /// </param>
        public CebTirage(int search, params int[] plaques) : this() {
            EventActif = false;
            if (plaques.Length < 6 || search == -1)
                Random();
            else {
                for (var i = 0; i < 6; i++) {
                    Plaques[i].Value = plaques[i];
                }
                Search = search;
            }
            EventActif = true;
        }

        private bool EventActif { get; set; }


        /// <summary>
        /// nombre � chercher
        /// </summary>
        public int Search {
            get => _search;
            set {
                if (value == _search) return;
                _search = value;
                Clear();
            }
        }

        /// <summary>
        /// Nombre de solutions
        /// </summary>
        public int Count => Solutions.Count;

        /// <summary>
        /// Ecart
        /// </summary>
        public int Diff { get; private set; } = int.MaxValue;
        public CebPlaque[] Plaques { get; } = new CebPlaque[6];

        public void SetPlaques(params int[] plaq) {
            EventActif = false;
            foreach (var (p ,i) in plaq.WithIndex().Where((p,i)=>i < 6)) {
                Plaques[i].Value = p;
            }
            
            Clear();
            EventActif = true;
        }

        public List<CebBase> Solutions { get; } = new List<CebBase>();

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <returns>
        /// </returns>
        /// <value>
        /// The status.
        /// </value>
        public CebStatus Status { get; private set; } = CebStatus.Indefini;

        /// <summary>
        /// Return the find values
        /// </summary>
        public CebFind Found { get; } = new CebFind();

        /// <summary>
        /// Valid
        /// </summary>
        public CebStatus Valid() {
            Status = SearchValid && PlaquesValid ? CebStatus.Valid : CebStatus.Erreur;
            return Status;
        }

        public CebBase Solution => Solutions.Count == 0 ? null : Solutions[0];

        /// <summary>
        /// Valid the search value
        /// </summary>
        /// <returns>
        /// </returns>
        public bool SearchValid => _search > 99 && _search < 1000;

        public bool PlaquesValid => Plaques.All(p => p.IsValid
                                                     && Plaques.Count(q => q.Value == p.Value) <=
                                                     CebPlaque.ListePlaques.Count(n => n == p.Value));

        /// <summary>
        /// Select the value and the plaque's list
        /// </summary>
        public void Random() {
            EventActif = false;
            var rnd = new Random();
            _search = rnd.Next(100, 1000);
            var liste = CebPlaque.ListePlaques.ToList();
            foreach (var plaque in Plaques) {
                var n = rnd.Next(0, liste.Count());
                plaque.Value = liste[n];
                liste.RemoveAt(n);
            }
            EventActif = true;
            Clear();
        }

        public async Task RandomAsync() => await Task.Run(() => Random());

        public void Clear() {
            Solutions.Clear();
            Diff = int.MaxValue;
            Found.Reset();
            Valid();
        }

        public async Task ClearAsync() => await Task.Run(() => Clear());

        /// <summary>
        /// resolution du compte
        /// </summary>
        /// <param name="search">
        /// Valeur � rechercher
        /// </param>
        /// <param name="plq">
        /// Liste des plaques
        /// </param>
        /// <returns>
        /// </returns>
        public CebStatus Resolve(int search, params int[] plq) {
            if (plq.Length != 6)
                throw new ArgumentException("Nombre de plaques incorrecte");
            _search = search;
            EventActif = false;
            foreach (var (p, i) in plq.WithIndex().Where((p,i) => i < 6))
                Plaques[i].Value = p; 
            //for (var i = 0; i < 6; i++)
            //    Plaques[i].Value = plq[i];
            EventActif = true;
            return Resolve();
        }
        private void UpdateSolutions(CebBase sol) {
            var diff = Abs(_search - sol.Value);
            if (diff > Diff)
                return;

            if (Solutions.Contains(sol))
                return;

            if (diff < Diff) {
                Diff = diff;
                Solutions.Clear();
                Found.Reset();
            }
            Found.Add(sol.Value);
            Solutions.Add(sol);
        }

        private void Resolve(List<CebBase> liste) {
            liste.Sort((p, q) => q.Value.CompareTo(p.Value));
            foreach(var (p, i) in liste.WithIndex())
            {
                UpdateSolutions(p);
                foreach (var (q, j) in liste.WithIndex().Where((q,j)=> j > i)) {
                    foreach (var oper in 
                        CebOperation.ListeOperations.Select(operation =>
                            CebOperation.@new(p, operation,q))
                                .Where(o=>o.IsValid)) {
                        Resolve(
                            liste.Where( (t,k)=> k!= i && k != j).Concat(new [] {oper}).ToList()
                        );

                    }
                }
            }
        }
        /// <summary>
        /// resolution
        /// </summary>
        /// <returns>
        /// </returns>
        public CebStatus Resolve() {
            Clear();
            if (Status != CebStatus.Valid) return Status;
            Resolve(Plaques.Cast<CebBase>().ToList());
            Solutions.Sort((p, q) => p.Rank.CompareTo(q.Rank));
            Status = Diff == 0 ? CebStatus.CompteEstBon : CebStatus.CompteApproche;
            return Status;
        }

        public async Task<CebStatus> ResolveAsync() => await Task.Run(() => Resolve());

        public async Task<CebStatus> ResolveAsync(int search, params int[] plq) => await Task.Run(() => Resolve(search, plq));

        public override string ToString() {
            var buffer = new StringBuilder();
            buffer.AppendLine("## Tirage du compte est bon ###");
            buffer.AppendLine($"Search : {Search}, plaques : [{string.Join(",", Plaques.Select(p => p.ToString()))}]");
            buffer.AppendLine($"Found:  {Found}, Status: {Status}, Nb de solutions: {Count}");
            buffer.AppendLine(string.Join(";", Solutions.Select((p) => p.ToString())));
            return buffer.ToString();
        }

        public IEnumerable<string> ToArray() => Solutions.Select(p => p.ToString());

        public IEnumerable<IEnumerable<string>> OperationsSolutions => Solutions.Select(p => p.Operations);
        public string[][] ArraysSolutions => Solutions.Select(p => p.Operations.ToArray()).ToArray();

        public IEnumerable<CebDetail> ToCebDetails() => Solutions.Select(s => s.ToCebDetail());

    }
    public static class CebUtils {
        public static IEnumerable<(T, int)> WithIndex<T>(this IEnumerable<T> self) => self.Select((value, index) => (value, index));
    }
}