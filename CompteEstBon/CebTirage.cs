#region using

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

/* Modification non fusionnée à partir du projet 'CompteEstBon_2.1'
Avant :
using System.Collections.ObjectModel;
using System.Collections.Specialized;
Après :
using System.Text;
using System.Threading.Tasks;
*/
using static System.Math;

#endregion using

namespace CompteEstBon {

    /// <summary>
    /// Gestion tirage Compte est bon
    /// </summary>
    [System.Runtime.InteropServices.Guid("EC9CF01C-34A0-414C-BF2A-D06C5A61503D")]
    public sealed class CebTirage : INotifyPropertyChanged {
        private int _search;

        public CebTirage() {
            for (var i = 0; i < 6; i++) {
                Plaques.Add(new CebPlaque(0, IsUpdated));
            }

            Random();
        }

        private void IsUpdated(object sender, PropertyChangedEventArgs e) {
            Clear("plaque");

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
                foreach (var (p, i) in plaques.WithIndex().Where(elt => elt.Item2  < 6)) {
                    Plaques[i].Value2 = p;
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
                Clear("Search");
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

        public List<CebPlaque> Plaques = new List<CebPlaque>();

        public void SetPlaques(params int[] plaq) {
            foreach (var (p, i) in plaq.WithIndex().Where(elt => elt.Item2 < 6)) {
                Plaques[i].Value2 = p;
            }

            Clear("Plaques");
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

        public void SetEncours() {
            Status = CebStatus.EnCours;
        }

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

    public string SolutionIndex(int no) {

            if (Solutions.Count == 0 || no >= Solutions.Count) return "";
            if (no < 0) no = 0;
            return Solutions[no].ToString();
        }

        /// <summary>
        /// Valid the search value
        /// </summary>
        /// <returns>
        /// </returns>
        public bool SearchValid => _search > 99 && _search < 1000;

        public bool PlaquesValid => Plaques.All(p => p.IsValid
                                                     && Plaques.Count(q => q.Value == p.Value) <= (p <= 25 ? 2 : 1));

        /// <summary>
        /// Select the value and the plaque's list
        /// </summary>
        public void Random() {
            Status = CebStatus.EnCours;
            _search = Rnd.Next(100, 1000);
            var liste = new List<int>(CebPlaque.ListePlaques); // .ToList();
            liste.AddRange(CebPlaque.ListePlaques.TakeWhile(v => v <= 25));
            foreach (var plaque in Plaques) {
                var n = Rnd.Next(0, liste.Count());
                plaque.Value2 = liste[n];
                liste.RemoveAt(n);
            }
            Clear("Random");
        }

        public async Task RandomAsync() => await Task.Run(Random);

        public void Clear(string evt = "") {
            if (Solutions.Count != 0) {
                Solutions.Clear();
                NotifiedChanged(evt);
            }

            Diff = int.MaxValue;
            Found.Reset();
            Valid();
        }


        public async Task ClearAsync() => await Task.Run(() => { Clear("Clear"); });

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
        public CebStatus ResolveWithParam(int search, params int[] plq) {
            if (plq.Length != 6)
                throw new ArgumentException("Nombre de plaques incorrecte");
            _search = search;
            foreach (var (p, i) in plq.WithIndex().Where(elt => elt.Item2 < 6))
                Plaques[i].Value2 = p;

            return Resolve();
        }
        private void UpdateSolutions(CebBase sol) {
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
            liste.Sort((p, q) => q.Value.CompareTo(p.Value));
            foreach (var (p, i) in liste.WithIndex()) {
                UpdateSolutions(p);
                foreach (var (q, j) in liste.WithIndex().Where(elt  => elt.Item2 > i)) {
                    foreach (var oper in
                        CebOperation.ListeOperations.Select(operation =>
                            new CebOperation(p, operation, q))
                                .Where(o => o.IsValid)) {
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
        public CebStatus Resolve() {

            Clear();
            if (Status != CebStatus.Valid) return Status;
            Status = CebStatus.EnCours;
            Resolve(Plaques.Cast<CebBase>().ToList());
            Solutions.Sort((p, q) => (p.Rank == q.Rank) ? p.Value.CompareTo(q.Value) : p.Rank.CompareTo(q.Rank));
            Status = Diff == 0 ? CebStatus.CompteEstBon : CebStatus.CompteApproche;

            NotifiedChanged($"Details");
            return Status;
        }

        public async Task<CebStatus> ResolveAsync() => await Task.Run(Resolve);

        public async Task<CebStatus> ResolveAsync(int search, params int[] plq) => await Task.Run(() => ResolveWithParam(search, plq));

        public override string ToString() {
            var buffer = new StringBuilder();
            buffer.AppendLine("## Tirage du compte est bon ###");
            buffer.AppendLine($"Search : {Search}, plaques : [{string.Join(",", Plaques.Select(p => p.ToString()))}]");
            buffer.AppendLine($"Found:  {Found}, Status: {Status}, Nb de solutions: {Count}");
            buffer.AppendLine(string.Join(";", Solutions.Select(p => p.ToString())));
            return buffer.ToString();
        }

        public IEnumerable<string> ToArray() => Solutions.Select(p => p.ToString());

        private static readonly Random Rnd = new Random();

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifiedChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public IEnumerable<CebDetail> Details => Solutions.Select(s => new CebDetail(s));
        public CebResult GetCebResult() => new CebResult {
            Search = this.Search,
            Plaques = Plaques.Select(p => p.Value),
            Status = this.Status,
            Diff = this.Diff,
            Solutions = Details,
            Found = this.Found.ToString()
        };
    }
    
}