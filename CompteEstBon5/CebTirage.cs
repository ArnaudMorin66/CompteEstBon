#region using

using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using static System.Math;

#endregion using

namespace CompteEstBon {
    // test siur 

    /// <summary>
    ///     Gestion tirage Compte est bon
    /// </summary>
    [Guid("EC9CF01C-34A0-414C-BF2A-D06C5A61503D")]
    public sealed class CebTirage : INotifyPropertyChanged {
        private static readonly Random Rnd = new();
        private readonly List<CebBase> _solutions = new(); // List<CebBase>();
        public readonly IEnumerable<int> ListePlaques = CebPlaque.AnyPlaques;
        private int _search;

        public CebTirage() {
            for (var i = 0; i < 6; i++) Plaques.Add(new CebPlaque(0, IsUpdated));
            Random();
        }

        /// <summary>
        ///     Constructeur Tirage du Compte est bon
        /// </summary>
        /// <param name="search">
        /// </param>
        /// <param name="plaques">
        /// </param>
        public CebTirage(int search, params int[] plaques) : this() {
            if (plaques.Length < 6 || search == -1) {
                Random();
            }
            else {
                Status = CebStatus.EnCours;
                foreach (var (p, i) in plaques.WithIndex().Where(elt => elt.Item2 < 6)) Plaques[i].Value = p;
                Search = search;
            }
        }

        /// <summary>
        ///     Nombre de solutions
        /// </summary>
        public int Count => _solutions.Count;

        /// <summary>
        ///     Ecart
        /// </summary>
        public int Diff { get; private set; } = int.MaxValue;

        /// <summary>
        ///     Return the find values
        /// </summary>
        public CebFind Found { get; } = new();

        public List<CebPlaque> Plaques { get; } = new();

        /// <summary>
        ///     nombre � chercher
        /// </summary>
        public int Search {
            get => _search;
            set {
                if (value == _search) return;
                _search = value;
                Clear();
            }
        }


        public ImmutableList<CebBase> Solutions =>
            Status is CebStatus.CompteApproche or CebStatus.CompteEstBon
                ? _solutions.ToImmutableList()
                : ImmutableList<CebBase>.Empty;

        [JsonIgnore] public IEnumerable<string> SolutionsToString => _solutions.Select(s => s.ToString());
        // public TimeSpan Duree { get; private set; }


        /// <summary>
        ///     Gets the status.
        /// </summary>
        /// <returns>
        /// </returns>
        /// <value>
        ///     The status.
        /// </value>
        public CebStatus Status { get; private set; } = CebStatus.Indefini;

        public Stopwatch Watch { get; } = new(); // Stopwatch();
        public double Duree => Watch.Elapsed.TotalSeconds;

        public event PropertyChangedEventHandler PropertyChanged;

        public CebData Clear() {
            if (_solutions.Count != 0) _solutions.Clear();
            // Duree = TimeSpan.Zero;
            Watch.Reset();
            Diff = int.MaxValue;
            Found.Reset();
            Valid();
            NotifyPropertyChanged();
            return GetData();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public async Task<CebData> ClearAsync() => await Task.Run(Clear);

        public CebData GetData() {
            return new() {
                Search = Search,
                Plaques = Plaques.Select(p => p.Value).ToArray(),
                Status = Status,
                Diff = Diff,
                Solutions = Solutions,
                Found = Found.ToString()
            };
        }

        public bool IsPlaquesValid() {
            return Plaques.All(p => p.IsValid
                                    && Plaques.Count(q => q.Value == p.Value) <=
                                    CebPlaque.AllPlaques.Count(i => i == p.Value));
        }

        /// <summary>
        ///     Valid the search value
        /// </summary>
        /// <returns>
        /// </returns>
        public bool IsSearchValid() => _search > 99 && _search < 1000;

        /// <summary>
        ///     Select the value and the plaque's list
        /// </summary>
        public CebData Random() {
            Status = CebStatus.EnCours;
            Search = Rnd.Next(100, 1000);
            var liste = new List<int>(CebPlaque.AllPlaques);
            Plaques.ForEach(p => {
                var n = Rnd.Next(0, liste.Count);
                p.Value = liste[n];
                liste.RemoveAt(n);
            });

            return Clear();
        }

        public async Task<CebData> RandomAsync() => await Task.Run(Random);

        /// <summary>
        ///     resolution
        /// </summary>
        /// <returns>
        /// </returns>
        public CebStatus Resolve() {
            _solutions.Clear();
            Watch.Reset();
            Watch.Start();
            Status = CebStatus.EnCours;
            Resolve(Plaques.ToList<CebBase>());
            _solutions.Sort((p, q) => p.Compare(q));
            Status = Diff == 0 ? CebStatus.CompteEstBon : CebStatus.CompteApproche;
            NotifyPropertyChanged();
            Watch.Stop();
            //Duree = watch.Elapsed;
            return Status;
        }

        public async Task<CebStatus> ResolveAsync() => await Task.Run(Resolve);

        public async Task<CebStatus> ResolveAsync(int search,
                                                  params int[] plq) => await Task.Run(() => ResolveWithParam(search, plq));

        /// <summary>
        ///     resolution du compte
        /// </summary>
        /// <param name="search">
        ///     Valeur � rechercher
        /// </param>
        /// <param name="plq">
        ///     Liste des plaques
        /// </param>
        /// <returns>
        /// </returns>
        public CebStatus ResolveWithParam(int search, params int[] plq) {
            if (plq.Length != 6)
                throw new ArgumentException("Nombre de plaques incorrecte");
            Status = CebStatus.EnCours;
            _search = search;
            foreach (var (p, i) in plq.WithIndex().Where((_, i) => i < 6))
                Plaques[i].Value = p;

            return Resolve();
        }

        public void SetPlaques(params int[] plaq) {
            Status = CebStatus.EnCours;
            foreach (var (p, i) in plaq.WithIndex().Where(elt => elt.Item2 < 6)) Plaques[i].Value = p;
            Clear();
        }

        public string Solution(int no = 0) => _solutions.Count == 0 || no < 0 || no >= _solutions.Count ? string.Empty : _solutions[no].ToString();

        /// <summary>
        ///     Valid
        /// </summary>
        public CebStatus Valid() {
            Status = IsSearchValid() && IsPlaquesValid() ? CebStatus.Valid : CebStatus.Erreur;
            return Status;
        }

        private void AddSolution(CebBase sol) {
            var diff = Abs(_search - sol.Value);
            if (diff > Diff)
                return;

            if (diff < Diff) {
                Diff = diff;
                _solutions.Clear();
                Found.Reset();
            }

            if (_solutions.Contains(sol)) return;

            Found.Add(sol.Value);
            _solutions.Add(sol);
        }

        private void IsUpdated(object sender, PropertyChangedEventArgs e) {
            if (Status != CebStatus.EnCours) Clear();
        }

        private void Resolve(IEnumerable<CebBase> liste) {
            // ReSharper disable PossibleMultipleEnumeration
            foreach (var (p1, i1) in liste.WithIndex()) {
                AddSolution(p1);
                foreach (var (p2, i2) in liste.WithIndex().Where((_, ix) => ix > i1)) {
                    foreach (var oper in
                        CebOperation.AllOperations.Select(operation =>
                                new CebOperation(p1, operation, p2))
                            .Where(o => o.Value != 0))
                        Resolve(
                            liste.Where((_, k) => k != i1 && k != i2).Concat(new[] { oper })
                        );
                }
            }
        }
    }
}