//-----------------------------------------------------------------------
// <copyright file="CebTirage.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// Plage Compte est bon
#region using
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using static System.Math;

#endregion using
#pragma warning disable CRRSP08
namespace CompteEstBon {
    // test siur 

    /// <summary>
    /// Gestion tirage Compte est bon
    /// </summary>


    public sealed class CebTirage : INotifyPropertyChanged {
        private static readonly Random Rnd = System.Random.Shared;
        private int _search;
        private readonly List<CebBase> _solutions = new();


        public CebTirage() {
            for(var i = 0; i < 6; i++) {
                CebPlaque p = new();
                p.PropertyChanging += PlaqueUpdated;
                Plaques[i] = p;
            }
            Random();
        }


        /// <summary>
        /// Constructeur Tirage du Compte est bon
        /// </summary>
        /// <param name="search"></param>
        /// <param name="plaques"></param>
        public CebTirage(int search, params int[] plaques) : this() {
            if(plaques.Length < 6 || search == -1) {
                Random();
            } else {
                Status = CebStatus.EnCours;
                foreach (var (p, i) in plaques.WithIndex().Where(elt => elt.Item2 < 6)) Plaques[i].Value = p;
                Search = search;
            }

            Resolve();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool IsPlaquesValid() => Plaques.All(
            p => p.IsValid && Plaques.Count(q => q.Value == p.Value) <= CebPlaque.AllPlaques.Count(i => i == p.Value));

        /// <summary>
        /// Valid the search value
        /// </summary>
        /// <returns>
        ///
        /// </returns>
        private bool IsSearchValid() => _search is > 99 and < 1000;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));

        private void PlaqueUpdated(object sender, PropertyChangingEventArgs args) {
            if(Status is not CebStatus.EnCours and not CebStatus.Indefini)
                Clear();
        }

        private void PushSolution(CebBase sol) {
            var diff = Abs(_search - sol.Value);
            if(diff > Diff)
                return;

            if(diff < Diff) {
                Diff = diff;
                _solutions.Clear();
                Found.Reset();
            }

            if(_solutions.Contains(sol))
                return;

            Found.Add(sol.Value);
            _solutions.Add(sol);
        }

        private void Resolve(IEnumerable<CebBase> liste) {
            // ReSharper disable PossibleMultipleEnumeration
            foreach (var (p, i) in liste.WithIndex()) {
                PushSolution(p);
                foreach (var (q, j) in liste.WithIndex().Where((_, ix) => ix > i))
                    foreach(var oper in
                             CebOperation.AllOperations
                        .Select(operation => new CebOperation(p, operation, q))
                        .Where(o => o.Value != 0))
                        Resolve(new[] { oper }.Concat(liste.Where((_, k) => k != i && k != j)));
            }
        }

        private Stopwatch Watch { get; } = new();

        public CebData Clear() {
            if(_solutions.Count != 0)
                _solutions.Clear();
            Watch.Reset();
            Diff = int.MaxValue;
            Found.Reset();
            Valid();
            NotifyPropertyChanged(nameof(Clear));
            return Data;
        }

        public async Task<CebData> ClearAsync() => await Task.Run(Clear);

        /// <summary>
        /// Select the value and the plaque's list
        /// </summary>
        public CebData Random() {
            Status = CebStatus.Indefini;

            var liste = new List<int>(CebPlaque.AllPlaques);
            foreach(var p in Plaques) {
                var n = Rnd.Next(0, liste.Count);
                p.Value = liste[n];
                liste.RemoveAt(n);
            }
            ;
            _search = Rnd.Next(100, 1000);
            return Clear();
        }

        public async Task<CebData> RandomAsync() => await Task.Run(Random);

        /// <summary>
        /// resolution
        /// </summary>
        /// <returns>
        ///
        /// </returns>
        public CebStatus Resolve() {
            _solutions.Clear();
            if(Status != CebStatus.Invalide) {
                Watch.Reset();
                Watch.Start();
                Status = CebStatus.EnCours;
                Resolve(Plaques.ToList<CebBase>());
                _solutions.Sort((p, q) => p.Compare(q));
                Status = Diff == 0 ? CebStatus.CompteEstBon : CebStatus.CompteApproche;
                Watch.Stop();
            }

            NotifyPropertyChanged(nameof(Resolve));
            return Status;
        }

        public async Task<CebStatus> ResolveAsync() => await Task.Run(Resolve);

        public async Task<CebStatus> ResolveAsync(int search, params int[] plq) => await Task.Run(
            () => ResolveWithParam(search, plq));

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
        ///
        /// </returns>
        public CebStatus ResolveWithParam(int search, params int[] plq) {
            if(plq.Length != 6)
                throw new ArgumentException("Nombre de plaques incorrecte");
            Status = CebStatus.Indefini;
            _search = search;
            foreach (var (p, i) in plq.WithIndex().Where((_, i) => i < 6))
                Plaques[i].Value = p;
            Valid();
            return Resolve();
        }

        public void SetPlaques(params int[] plaq) {
            Status = CebStatus.Indefini;
            foreach(var p in Plaques)
                p.Value = 0;
            foreach (var (p, i) in plaq.WithIndex().Where(elt => elt.Item2 < 6)) Plaques[i].Value = p;
            Clear();
        }

        public string Solution(int no = 0) => _solutions.Count == 0 || no < 0 || no >= _solutions.Count
            ? string.Empty
            : _solutions[no].ToString();

        /// <summary>
        /// Valid
        /// </summary>
        public CebStatus Valid() {
            Status = IsSearchValid() && IsPlaquesValid() ? CebStatus.Valide : CebStatus.Invalide;
            return Status;
        }

        /// <summary>
        /// Nombre de solutions
        /// </summary>
        public int Count => _solutions.Count;

        public CebData Data => new()
        {
            Search = Search,
            Plaques = Plaques.Select(p => p.Value).ToArray(),
            Status = Status,
            Diff = Diff,
            Solutions = Solutions?.Select(p => p.ToString()).ToArray(),
            Found = Found.ToString()
        };

        /// <summary>
        /// Ecart
        /// </summary>
        public int Diff { get; private set; } = int.MaxValue;

        public double Duree => Watch.Elapsed.TotalSeconds;

        /// <summary>
        /// Return the find values
        /// </summary>
        public CebFind Found { get; } = new();

        public CebPlaque[] Plaques { get; } = new CebPlaque[6];

        /// <summary>
        /// nombre � chercher
        /// </summary>
        public int Search {
            get => _search;
            set {
                if(value == _search)
                    return;
                _search = value;
                Clear();
            }
        }


        public List<CebBase>? Solutions => Status is CebStatus.CompteApproche or CebStatus.CompteEstBon
            ? _solutions
            : null; // new List<CebBase>();


        // [JsonIgnore] public IEnumerable<string> SolutionsToString => _solutions.Select(s => s.ToString());


        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <returns>
        ///
        /// </returns>
        /// <value>
        /// The status.
        /// </value>
        public CebStatus Status { get; private set; } = CebStatus.Indefini;
    }
}