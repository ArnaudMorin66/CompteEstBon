#region using

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
                Plaques.Add(new CebPlaque(0, IsUpdated));
            }
            Random();
        }

        private void IsUpdated(object sender, PropertyChangedEventArgs e) {
            if (Status != CebStatus.EnCours) {
                Clear();
            }
        }

        /// <summary>
        /// Constructeur Tirage du Compte est bon
        /// </summary>
        /// <param name="search">
        /// </param>
        /// <param name="plaques">
        /// </param>
        public CebTirage(int search, params int[] plaques) : this() {
            if (plaques.Length < 6 || search == -1)
                Random();
            else {
                Status = CebStatus.EnCours;
                foreach (var (p, i) in plaques.WithIndex().Where(elt => elt.Item2 < 6)) {
                    Plaques[i].Value = p;
                }
                Search = search;
            }
        }

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
        public List<CebPlaque> Plaques { get; } = new List<CebPlaque>();
        public void SetPlaques(params int[] plaq) {
            Status = CebStatus.EnCours;
            foreach (var (p, i) in plaq.WithIndex().Where(elt => elt.Item2 < 6)) {
                Plaques[i].Value = p;
            }
            Clear();
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
            Status = IsSearchValid() && IsPlaquesValid() ? CebStatus.Valid : CebStatus.Erreur;
            return Status;
        }
        public string Solution(int no = 0) => Solutions.Count == 0 || no < 0 || no >= Solutions.Count ? "" : Solutions[no].ToString();

        /// <summary>
        /// Valid the search value
        /// </summary>
        /// <returns>
        /// </returns>
        public bool IsSearchValid() => _search > 99 && _search < 1000;
        public bool IsPlaquesValid() => Plaques.All(p => p.IsValid
                                         && Plaques.Count(q => q.Value == p.Value) <= CebPlaque.AllPlaques.Count(i => i == p.Value));
        /// <summary>
        /// Select the value and the plaque's list
        /// </summary>
        public CebData Random() {
            Status = CebStatus.EnCours;
            _search = Rnd.Next(100, 1000);
            var liste = new List<int>(CebPlaque.AllPlaques);
            foreach (var plaque in Plaques) {
                var n = Rnd.Next(0, liste.Count());
                plaque.Value = liste[n];
                liste.RemoveAt(n);
            }
            return Clear();
            
        }
        public async Task<CebData> RandomAsync() => await Task.Run(Random);

        public CebData Clear() {
            if (Solutions.Count != 0) {
                Solutions.Clear();
            }

            Diff = int.MaxValue;
            Found.Reset();
            Valid();
            return Result;
        }
        public async Task<CebData> ClearAsync() => await Task.Run(Clear );

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
        public CebData ResolveWithParam(int search, params int[] plq) {
            if (plq.Length != 6)
                throw new ArgumentException("Nombre de plaques incorrecte");
            Status = CebStatus.EnCours;
            _search = search;
            foreach (var (p, i) in plq.WithIndex().Where((_,i) => i < 6))
                Plaques[i].Value = p;

            return Resolve();
        }
        private void AddSolution(CebBase sol) {

            var diff = Abs(_search - sol.Value);
            if (diff > Diff)
                return;

            if (diff < Diff) {
                Diff = diff;
                Solutions.Clear();
                Found.Reset();
            }
            if (Solutions.Contains(sol)) return;

            Found.Add(sol.Value);
            Solutions.Add(sol);
        }

        private void Resolve(List<CebBase> liste) {
            // liste.Sort((a,b) => b.Value.CompareTo(a.Value));
            foreach (var (p, i) in liste.WithIndex()) {
                AddSolution(p);
                foreach (var (q, j) in liste.WithIndex().Where((_, ix) => ix > i)) {
                    foreach (var oper in
                        CebOperation.AllOperations.Select(operation =>
                            new CebOperation(p, operation, q))
                                .Where(o => o.Value != 0)) {
                        Resolve(
                            liste.Where((_, k) => k != i && k != j).Concat(new[] { oper }).ToList()
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
        public CebData Resolve() {
            Clear();
            if (Status != CebStatus.Valid) return Result;
            Status = CebStatus.EnCours;
            Resolve(Plaques.ToList<CebBase>());
            Solutions.Sort((p, q) => p.Compare(q));
            Status = Diff == 0 ? CebStatus.CompteEstBon : CebStatus.CompteApproche;
            return Result;
        }
        public async Task<CebData> ResolveAsync() => await Task.Run(Resolve);
        public async Task<CebData> ResolveAsync(int search, params int[] plq) => await Task.Run(() => ResolveWithParam(search, plq));
        private static readonly Random Rnd = new Random();
       
        public IEnumerable<string> SolutionsToString => Solutions.Select(s => s.ToString());
        
        public CebData Result => new CebData {
            Search = Search,
            Plaques = Plaques.Select(p => p.Value),
            Status = Status,
            Diff = Diff,
            Solutions = Solutions,
            Found = Found.ToString()
        };
        public IEnumerable<int> ListePlaques = CebPlaque.AnyPlaques;
    }

}