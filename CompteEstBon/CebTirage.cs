//-----------------------------------------------------------------------
// <copyright file="CebTirage.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#region using
using System.Collections.Immutable;
using System.ComponentModel;

using System.Runtime.CompilerServices;

using arnaud.morin.outils;

using static System.Math;

#endregion using
namespace CompteEstBon;

#nullable enable
/// <summary>
/// Gestion tirage Compte est bon
/// </summary>
public sealed class CebTirage : INotifyPropertyChanged {
    private static readonly Random Rnd = System.Random.Shared;

    private int _search;

    private readonly List<CebBase> _solutions = new();

    /// <summary>
    /// <param name="n">Nombre de plaques du tirage</param>
    /// </summary>
    public CebTirage(int n = 6) {
        Plaques = new CebPlaque[n];
        Random();
    }

    /// <summary>
    /// Constructeur Tirage du Compte est bon
    /// </summary>
    /// <param name="search"></param>
    /// <param name="plaques"></param>
    public CebTirage(int n, int search, params int[] plaques) : this(n) {
        if(plaques.Length != 0)
            SetPlaques(plaques);
        Search = search;
    }

    /// <summary>
    ///
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public CebBase? this[int i] => Solutions?[i];
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    private bool IsPlaquesValid() => Plaques.All(
        p => p.IsValid && Plaques.Count(q => q.Value == p.Value) <= CebPlaque.ListePlaques.Count(i => i == p.Value));

    /// <summary>
    /// Valid the search value
    /// </summary>
    /// <returns>
    ///
    /// </returns>
    private bool IsSearchValid() => _search is >= 100 and <= 999;

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
        if(Status is CebStatus.EnCours or CebStatus.Indefini)
            return;
        Clear();
    }


    /// <summary>
    /// Remise à zéro des données
    /// </summary>
    /// <returns></returns>
    public CebStatus Clear() {
        _solutions.Clear();
        Duree = TimeSpan.Zero;
        Ecart = null;
        Status = CebStatus.Indefini;
        Valid();
        NotifyPropertyChanged("clear");
        return Status;
    }

    /// <summary>
    /// Remise à zéro asynchrone
    /// </summary>
    /// <returns></returns>
    public async Task<CebStatus> ClearAsync() => await Task.Run(() => Clear());

    /// <summary>
    /// Select the value and the plaque's list
    /// </summary>
    public CebStatus Random() {
        Status = CebStatus.Indefini;
        List<int> liste = new(CebPlaque.ListePlaques);

        for(var i = 0; i < Plaques.Length; i++) {
            var n = Rnd.Next(0, liste.Count);
            Plaques[i] = new(liste[n]);
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
    /// Insertion de la solution en cours
    /// </summary>
#region resolution
    /// <param name="sol"></param>
    private void InsertSolution(CebBase sol) {
        var ecart = Abs(_search - sol.Value);
        switch(ecart - Ecart) {
            case > 0:
                return;
            case 0: {
                if(_solutions.Contains(sol))
                    return;
                break;
            }
            case < 0:
                Ecart = ecart;
                _solutions.Clear();
                break;
        }
        _solutions.Add(sol);
    }


    /// <summary>
    /// boucle de résolution du compte
    /// </summary>
    /// <param name="liste"></param>
    private void Resolve(IEnumerable<CebBase> liste) {
        foreach (var (g, i) in liste.Indexed()) {
            InsertSolution(g);
            foreach (var (d, j) in liste.Indexed().Where((_, j) => j > i))
                foreach(var oper in CebOperation.ListeOperations
                    .Select(op => new CebOperation(g, op, d))
                    .Where(o => o.Value != 0))
                    Resolve(liste.Where((_, k) => k != i && k != j).Concat(new[] { oper }));
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public CebStatus Resolve() {
        _solutions.Clear();
        if(Status == CebStatus.Invalide)
            return Status;
        var debut = DateTime.Now;
        Status = CebStatus.EnCours;
        Ecart = int.MaxValue;
        Resolve(Plaques);
        Status = Ecart == 0 ? CebStatus.CompteEstBon : CebStatus.CompteApproche;
        _solutions.Sort((p, q) => p.Rank.CompareTo(q.Rank));

        Duree = DateTime.Now - debut;
        NotifyPropertyChanged(nameof(Solutions));
        return Status;
    }


    /// <summary>
    /// resolution du compte
    /// </summary>
    /// <param name="search">
    /// Valeur à rechercher
    /// </param>
    /// <param name="plq">
    /// Liste des plaques
    /// </param>
    /// <returns>
    ///
    /// </returns>
    public CebStatus Resolve(int search, params int[] plq) {
        Status = CebStatus.Indefini;
        _search = search;
        SetPlaques(plq);
        return Resolve();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public async Task<CebStatus> ResolveAsync() => await Task.Run(Resolve);

    /// <summary>
    /// Résoudre en asynchrone avec paramètres
    /// </summary>
    /// <param name="search"></param>
    /// <param name="plq"></param>
    /// <returns></returns>
    public async Task<CebStatus> ResolveAsync(int search, params int[] plq) => await Task.Run(
        () => Resolve(search, plq));
    #endregion

    /// <summary>
    /// resolution
    /// </summary>
    /// <returns>
    ///
    /// </returns>
    public void SetPlaques(params int[] plaq) {
        Status = CebStatus.Indefini;
        if(plaq.Length != Plaques.Length) {
            foreach(var plaque in Plaques)
                plaque.Value = 0;
            Clear();
            return;
        }

        foreach (var (p, i) in plaq.Indexed()) Plaques[i].Value = p;
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
    /// <param name="no"></param>
    /// <returns></returns>
    public string? Solution(int no = 0) => no >= 0 && no < Solutions?.Count ? Solutions![no].ToString() : null;

    /// <summary>
    /// Valid
    /// </summary>
    public CebStatus Valid() {
        if(Status is CebStatus.CompteEstBon or CebStatus.CompteApproche)
            return Status;
        Status = IsSearchValid() && IsPlaquesValid() ? CebStatus.Valide : CebStatus.Invalide;
        return Status;
    }

    /// <summary>
    /// Nombre de solutions
    /// </summary>
    public int Count => Solutions?.Count ?? 0;

    /// <summary>
    /// Ecart
    /// </summary>
    public int? Ecart { get; private set; } = null;

    /// <summary>
    ///
    /// </summary>
    public TimeSpan Duree { get; private set; }

    /// <summary>
    ///
    /// </summary>
    public string FirstSolution => Solutions?.First().ToString();

    /// <summary>
    ///
    /// </summary>
    public string? Found {
        get {
            int mn = Solutions?.Min(sol => sol.Value) ?? int.MaxValue;
            if(mn == int.MaxValue)
                return null;
            int mx = Solutions.Max(sol => sol.Value);
            return $"{mn}{(mn == mx ? string.Empty : $" et {mx}")}";
        }
    }

    /// <summary>
    /// Liste des plaques
    /// </summary>
    public CebPlaque[] Plaques { get; }

    /// <summary>
    ///
    /// </summary>
    public CebData Resultat => new()
    {
        Search = Search,
        Plaques = Plaques.Select(p => p.Value).ToArray(),
        Status = Status.ToString(),
        Diff = Ecart,
        Solutions = Solutions?.Select(p => p.ToString()).ToArray(),
        Found = Found
    };

    /// <summary>
    /// nombre à chercher
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

    /// <summary>
    ///
    /// </summary>
    public ImmutableList<CebBase>? Solutions => (Status is CebStatus.CompteEstBon or CebStatus.CompteApproche)
        ? _solutions.ToImmutableList()
        : null;


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