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
using arnaud.morin.outils;
using static System.Math;

#endregion using

namespace CompteEstBon;

/// <summary>
///     Gestion tirage Compte est bon
/// </summary>
public sealed class CebTirage : INotifyPropertyChanged {
    private static readonly Random Rnd = System.Random.Shared;

    private readonly List<CebBase> _solutions = new();

    /// <summary>
    ///
    /// <param name="nplaques">Nombre de plaques du tirage</param>
    /// </summary>
    public CebTirage(int nplaques = 6) {
        Nplaques = nplaques;
        Plaques = new CebPlaque[Nplaques];
        Random();
    }


    /// <summary>
    ///     Constructeur Tirage du Compte est bon
    /// </summary>
    /// <param name="search"></param>
    /// <param name="plaques"></param>
    public CebTirage(int nplaques, int search, params int[] plaques) : this(nplaques) {
        if (plaques.Length != 0) SetPlaques(plaques);
        Search = search;
        Clear();
    }
    /// <summary>
    /// 
    /// </summary>
    public int Nplaques { get; private set; }
    /// <summary>
    /// 
    /// </summary>
    public CebBase[]? Solutions { get; private set; }
    /// <summary>
    /// 
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool IsPlaquesValid() => Plaques.All(
        p => p.IsValid && Plaques.Count(q => q.Value == p.Value) <= CebPlaque.ListePlaques.Count(i => i == p.Value));

    /// <summary>
    ///     Valid the search value
    /// </summary>
    /// <returns>
    /// </returns>
    private bool IsSearchValid() => _search is > 99 and < 1000;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(
        this,
        new PropertyChangedEventArgs(propertyName));
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void PlaqueUpdated(object sender, PropertyChangedEventArgs args) {
        if (Status is CebStatus.EnCours or CebStatus.Indefini) return;
        Clear();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public CebStatus Resolve() {
        _solutions.Clear();
        Solutions = null;
        if (Status != CebStatus.Invalide) {
            var debut = DateTime.Now;
            Status = CebStatus.EnCours;
            Resolve(Plaques);
            Status = Diff == 0 ? CebStatus.CompteEstBon : CebStatus.CompteApproche;
            Solutions = _solutions.OrderBy(b=> b.Rank).ToArray();
            Duree = DateTime.Now - debut;
        }

        NotifyPropertyChanged();
        return Status;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<CebStatus> ResolveAsync() => await Task.Run(Resolve);


    /// <summary>
    ///     resolution du compte
    /// </summary>
    /// <param name="search">
    ///     Valeur à rechercher
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="search"></param>
    /// <param name="plq"></param>
    /// <returns></returns>
    public async Task<CebStatus> ResolveAsync(int search, params int[] plq) => await Task.Run(
        () => ResolveWithParam(search, plq));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sol"></param>
    private void InsertSolution(CebBase sol) {
        var diff = Abs(_search - sol.Value);
        switch (diff - Diff) {
            case > 0:
                return;
            case 0: {
                if (_solutions.Contains(sol)) return;
                break;
            }
            case < 0:
                Diff = diff;
                _solutions.Clear();
                Found.Reset();
                break;
        }

        Found.Add(sol.Value);
        _solutions.Add(sol);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="liste"></param>
    private void Resolve(IEnumerable<CebBase> liste) {
        // ReSharper disable PossibleMultipleEnumeration
        foreach (var (p, i) in liste.WithIndex()) {
            InsertSolution(p);
            foreach (var (q, j) in liste.WithIndex().Where((_, j) => j > i))
                foreach (var oper in CebOperation.AllOperations.Select(operation => 
                                 new CebOperation(p, operation, q)).Where(o => o.Value != 0))
                    Resolve(liste.Where((_, k) => k  != i && k != j).Concat(new[]{ oper}));
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<CebStatus> ClearAsync() => await Task.Run(Clear);

    /// <summary>
    ///     Select the value and the plaque's list
    /// </summary>
    public CebStatus Random() {
        Status = CebStatus.Indefini;
        var liste = new List<int>(CebPlaque.ListePlaques);
        for (var i = 0; i < Nplaques; i++) {
            var n = Rnd.Next(0, liste.Count);
            Plaques[i] = new CebPlaque(liste[n]);
            Plaques[i].PropertyChanged += PlaqueUpdated;
            liste.RemoveAt(n);
        }

        _search = Rnd.Next(100, 1000);
        return Clear();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<CebStatus> RandomAsync() => await Task.Run(Random);

    /// <summary>
    ///     resolution
    /// </summary>
    /// <returns>
    /// </returns>
    public void SetPlaques(params int[] plaq) {
        Status = CebStatus.Indefini;
        if (plaq.Length != Nplaques) {
            foreach (var plaque in Plaques) plaque.Value = 0;
            Clear();
            return;
        }

        foreach (var (p, i) in plaq.WithIndex()) Plaques[i].Value = p;
        Clear();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pq"></param>
    public void SetPlaques(IList<int> pq) => SetPlaques(pq.ToArray());

    /// <summary>
    /// 
    /// </summary>
    public string FirstSolution => Solutions?.First().ToString();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="no"></param>
    /// <returns></returns>
    public string? Solution(int no = 0) => no >= 0 && no < Solutions?.Length ? Solutions![no].ToString() : null;

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
    public int Count => Solutions?.Length ?? 0;
    /// <summary>
    /// 
    /// </summary>
    public CebData Result =>
        new() {
            Search = Search,
            Plaques = Plaques.Select(p=> p.Value).ToArray(),
            Status = Status.ToString(),
            Diff = Diff,
            Solutions = Solutions?.Select(p => p.ToString()),
            Found = Found.ToString()
        };

    /// <summary>
    ///     Ecart
    /// </summary>
    public int Diff { get; private set; } = int.MaxValue;
    /// <summary>
    /// 
    /// </summary>
    public TimeSpan Duree { get; private set; }


    /// <summary>
    ///     Return the find values
    /// </summary>
    public CebFind Found { get; } = new();

    /// <summary>
    ///     Liste des plaques
    /// </summary>
    public CebPlaque[] Plaques { get; }

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


    /// <summary>
    ///     Gets the status.
    /// </summary>
    /// <returns>
    /// </returns>
    /// <value>
    ///     The status.
    /// </value>
    public CebStatus Status { get; private set; } = CebStatus.Indefini;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public CebBase? this[int i] => Solutions?[i];
}