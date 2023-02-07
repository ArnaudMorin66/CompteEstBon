//-----------------------------------------------------------------------
// <copyright file="CebTirage.cs" company="">
//     Author:  Arnaud Morin
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// Plage Compte est bon

#region using

using System.ComponentModel;
using System.Runtime.CompilerServices;
using static System.Math;

#endregion using

namespace CompteEstBon;

/// <summary>
///     Gestion tirage Compte est bon
/// </summary>
public sealed class CebTirage : INotifyPropertyChanged {
    public static int NbPlaques { get; set; } = 6;
    private static readonly Random Rnd = System.Random.Shared;
    
    
    public CebTirage() {
        Plaques = new List<CebPlaque>();
        Random();
    }


    /// <summary>
    ///     Constructeur Tirage du Compte est bon
    /// </summary>
    /// <param name="search"></param>
    /// <param name="plaques"></param>
    public CebTirage(int search, params int[] plaques) : this() {
        if (plaques.Length != 0) SetPlaques(plaques);
        Search = search;
        Clear();
    }
    
    private readonly List<CebBase> _solutions = new();
    public List<CebBase>? Solutions { get; private set; }

    public event PropertyChangedEventHandler PropertyChanged;

    private bool IsPlaquesValid() => Plaques.All(
        p => p.IsValid && Plaques.Count(q => q.Value == p.Value) <= CebPlaque.AllPlaques.Count(i => i == p.Value));

    /// <summary>
    ///     Valid the search value
    /// </summary>
    /// <returns>
    /// </returns>
    private bool IsSearchValid() => _search is > 99 and < 1000;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(
        this,
        new PropertyChangedEventArgs(propertyName));

    private void PlaqueUpdated(object sender, PropertyChangedEventArgs args) {
        if (Status is CebStatus.EnCours or CebStatus.Indefini) return;
        Clear();
    }

    private void InsertSolution(CebBase sol) {
        var diff = Abs(_search - sol.Value);
        switch (diff - Diff) {
            case 0: {
                if (_solutions.Contains(sol)) return;
                break;
            }
            case < 0:
                Diff = diff;
                _solutions.Clear();
                Found.Reset();
                break;
            default:
                return;
        }

        Found.Add(sol.Value);
        _solutions.Add(sol);
    }

    private void Resolve(IEnumerable<CebBase> liste) {
        // ReSharper disable PossibleMultipleEnumeration
        foreach (var (p, i) in liste.WithIndex()) {
            InsertSolution(p);
            foreach (var (q, j) in liste.WithIndex().Where((_, ix) => ix > i))
                foreach (var oper in CebOperation.AllOperations.Select(
                             operation => new CebOperation(p, operation, q)).Where(o => o.Value != 0))
                    Resolve(new[] { oper }.Concat(liste.Where((_, k) => k != i && k != j)));
        }
    }

    // private Stopwatch Watch { get; } = new();

    public CebStatus Clear() {
        Solutions = null;
        _solutions.Clear();
        Duree = TimeSpan.Zero;
        Diff = int.MaxValue;
        Found.Reset();
        Status = CebStatus.Indefini;
        Valid();
        NotifyPropertyChanged();
        return Status;
    }

    public async Task<CebStatus> ClearAsync() => await Task.Run(Clear);

    /// <summary>
    ///     Select the value and the plaque's list
    /// </summary>
    public CebStatus Random() {
        Status = CebStatus.Indefini;
        Plaques.Clear();

        var liste = new List<int>(CebPlaque.AllPlaques);
        while (Plaques.Count < NbPlaques) {
            var n = Rnd.Next(0, liste.Count);
            var pq = new CebPlaque(liste[n]);
            pq.PropertyChanged += PlaqueUpdated;
            Plaques.Add(pq);
            liste.RemoveAt(n);
        }
        _search = Rnd.Next(100, 1000);
        return Clear();
    }

    public async Task<CebStatus> RandomAsync() => await Task.Run(Random);

    /// <summary>
    ///     resolution
    /// </summary>
    /// <returns>
    /// </returns>
    public CebStatus Resolve() {
        _solutions.Clear();
        if (Status != CebStatus.Invalide) {
            var debut = DateTime.Now;
            Status = CebStatus.EnCours;
            Resolve(Plaques);
            _solutions.Sort((p, q) => p.Compare(q));
            Status = Diff == 0 ? CebStatus.CompteEstBon : CebStatus.CompteApproche;
            Solutions = _solutions;
            Duree = (DateTime.Now - debut);
        }

        NotifyPropertyChanged();
        return Status;
    }

    public async Task<CebStatus> ResolveAsync() => await Task.Run(Resolve);

    public async Task<CebStatus> ResolveAsync(int search, params int[] plq) => await Task.Run(
        () => ResolveWithParam(search, plq));

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
        Status = CebStatus.Indefini;
        _search = search;
        SetPlaques(plq);
        return Resolve();
    }


    public void SetPlaques(params int[] plaq) {
        Status = CebStatus.Indefini;
        if (plaq.Length != NbPlaques) {
            foreach (var plaque in Plaques) plaque.Value = 0;
            Clear();
            return;
        }

        foreach (var (p, i) in plaq.WithIndex().Where(elt => elt.Item2 < 6)) Plaques[i].Value = p;
        Clear();
    }

    public void SetPlaques(IList<int> pq) => SetPlaques(pq.ToArray());
    public string FirstSolution => Solution();

    public string Solution(int no = 0) => Count == 0 || no < 0 || no >= Count
        ? string.Empty
        : Solutions![no].ToString();

    /// <summary>
    ///     Valid
    /// </summary>
    public CebStatus Valid() {
        if (Status is CebStatus.CompteEstBon or CebStatus.CompteApproche) return Status;
        Status = IsSearchValid() && IsPlaquesValid() ? CebStatus.Valide : CebStatus.Invalide;
        return Status;
    }

    /// <summary>
    ///     Nombre de solutions
    /// </summary>
    public int Count => Solutions?.Count ?? 0;

    public CebData Result =>
        new() {
            Search = Search,
            Plaques = Plaques.Select(p => p.Value).ToArray(),
            Status = Status.ToString(),
            Diff = Diff,
            Solutions = Solutions?.Select(p => p.ToString()).ToArray() ?? Array.Empty<string>(),
            Found = Found.ToString()
        };

    /// <summary>
    ///     Ecart
    /// </summary>
    public int Diff { get; private set; } = int.MaxValue;

    public TimeSpan Duree { get; private set; }


    /// <summary>
    ///     Return the find values
    /// </summary>
    public CebFind Found { get; } = new();

    /// <summary>
    ///  Liste des plaques
    /// </summary>
    public List<CebPlaque> Plaques { get; }

    private int _search;
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


    // public List<CebBase> Solutions { get; private set; }


    /// <summary>
    ///     Gets the status.
    /// </summary>
    /// <returns>
    /// </returns>
    /// <value>
    ///     The status.
    /// </value>
    public CebStatus Status { get; private set; } = CebStatus.Indefini;
}