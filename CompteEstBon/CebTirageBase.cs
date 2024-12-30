//-----------------------------------------------------------------------
// <copyright file="CebTirage.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#region using

using System.ComponentModel;

using arnaud.morin.outils;

using static System.Math;

#endregion using

namespace CompteEstBon;

#nullable enable
/// <summary>
///     Gestion tirage Compte est bon
/// </summary>
public class CebTirageBase {
    private static readonly Random Rnd = System.Random.Shared;
    protected readonly List<CebBase> _solutions = [];

    /// <summary>
    ///     <param name="n">Nombre de plaques du tirage</param>
    /// </summary>
    public CebTirageBase(int n = 6) {
        Plaques = Enumerable.Range(0, n).Select(_ => new CebPlaque()).ToArray();
        SearchValue = new CebSearchValue();
        Random();
    }

    /// <summary>
    ///     Constructeur Tirage du Compte est bon
    /// </summary>
    /// <param name="n"></param>
    /// <param name="search"></param>
    /// <param name="plaques"></param>
    public CebTirageBase(int n, int search, params int[] plaques) : this(n) {
        if (plaques.Length != 0) SetPlaques(plaques);
        Search = search;
    }

    /// <summary>
    ///     Solution à la position i
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public CebBase this[int i] => Solutions[i];

    /// <summary>
    ///     Nombre de solutions
    /// </summary>
    public int Count => Solutions.Count;

    /// <summary>
    ///     Ecart
    /// </summary>
    public int? Ecart { get; protected set; }

    /// <summary>
    ///     Première solution
    /// </summary>
    public string FirstSolution => Solutions.First().ToString()!;

    /// <summary>
    ///     Renvoie le ou les valeurs trouvées
    /// </summary>
    public string Found => Solutions.Select(s => s.Value.ToString()).Distinct().Order().Join(", ");

    /// <summary>
    ///     Liste des plaques
    /// </summary>
    public CebPlaque[] Plaques { get; }

    /// <summary>
    ///     Retourne le résultat
    /// </summary>
    /// />
    public CebData Resultat => new() {
        Search = Search,
        Plaques = Plaques.Select(p => p.Value).ToArray(),
        Status = Status.ToString(),
        Ecart = Ecart,
        Solutions = Solutions.Select(p => p.ToString()).ToList(),
        Found = Found
    };

    public CebSearchValue SearchValue { get; }

    /// <summary>
    ///     nombre à chercher
    /// </summary>
    public int Search {
        get => SearchValue.Value;
        set {
            if (value == SearchValue.Value) return;
            SearchValue.Value = value;
            Clear();
        }
    }

    /// <summary>
    ///     Liste des soulutions
    /// </summary>
    public List<CebBase> Solutions => Status is CebStatus.CompteEstBon or CebStatus.CompteApproche
        ? _solutions
        : new List<CebBase>();

    /// <summary>
    ///     Gets the status.
    /// </summary>
    /// <returns>
    /// </returns>
    /// <value>
    ///     The status.
    /// </value>
    public CebStatus Status { get; protected set; } = CebStatus.Indefini;

    /// <summary>
    ///     Retourne si la plaque est valide
    /// </summary>
    /// <returns></returns>
    private bool IsPlaquesValid() => Plaques.All(
        p => p.IsValid && Plaques.Count(q => q.Value == p.Value) <= CebPlaque.ListePlaques.Count(i => i == p.Value));

    /// <summary>
    ///     Valid the search value
    /// </summary>
    /// <returns>
    /// </returns>
    private bool IsSearchValid() => Search is >= 100 and <= 999;

    /// <summary>
    ///     Si la plaque est modifiée
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnUpdateData(object sender, PropertyChangedEventArgs _) {
        if (Status is not (CebStatus.EnCours or CebStatus.Indefini)) Clear();
    }

    /// <summary>
    ///     Remise à zéro des données
    /// </summary>
    /// <returns></returns>
    public virtual CebStatus Clear() {
        _solutions.Clear();
        Ecart = null;
        Status = CebStatus.Indefini;
        Valid();
        return Status;
    }

    /// <summary>
    ///     Remise à zéro asynchrone
    /// </summary>
    /// <returns></returns>
    public async ValueTask<CebStatus> ClearAsync() => await Task.Run(Clear);

    /// <summary>
    ///   Disconnects the plaques from the event handler.
    /// </summary>
    public void DisconnectPlaques() {
        foreach (var plaque in Plaques) plaque.PropertyChanged -= OnUpdateData!;
    }

    /// <summary>
    ///    Connects the plaques to the event handler.
    /// </summary>
    public void ConnectPlaques() {
        foreach (var plaque in Plaques) plaque.PropertyChanged += OnUpdateData!;
    }

    /// <summary>
    ///    Connects the search value to the event handler.
    /// </summary>
    public void ConnectSearch() => SearchValue.PropertyChanged += OnUpdateData!;

    /// <summary>
    ///    Disconnects the search value from the event handler.
    /// </summary>
    public void DisconnectSearch() => SearchValue.PropertyChanged -= OnUpdateData!;

    /// <summary>
    ///     Connects all plaques and search value to their respective event handlers.
    /// </summary>
    public void ConnectAll() {
        ConnectPlaques();
        ConnectSearch();
    }

    /// <summary>
    ///    Disconnects all plaques and search value from their respective event handlers.
    /// </summary>
    public void DisconnectAll() {
        DisconnectPlaques();
        DisconnectSearch();
    }

    /// <summary>
    ///     Select the value and the plaque's list
    /// </summary>
    public CebStatus Random() {
        DisconnectAll();
        Status = CebStatus.Indefini;
        List<int> liste = [.. CebPlaque.ListePlaques];


        foreach (var t in Plaques) {
            var n = Rnd.Next(0, liste.Count);
            t.Value = liste[n];
            liste.RemoveAt(n);
        }

        Search = Rnd.Next(100, 1000);
        ConnectAll();
        return Clear();
    }

    /// <summary>
    ///     Asynchronously selects the value and the plaque's list.
    /// </summary>
    /// <returns>
    ///     A <see cref="CebStatus" /> representing the status of the operation.
    /// </returns>
    /// <remarks>
    ///     This method runs the <see cref="Random" /> method on a separate thread.
    /// </remarks>
    public async ValueTask<CebStatus> RandomAsync() => await Task.Run(Random);

    /// <summary>
    ///     resolution
    /// </summary>
    /// <returns>
    /// </returns>
    public void SetPlaques(params int[] plaq) {
        Status = CebStatus.Indefini;
        DisconnectPlaques();
        if (plaq.Length != Plaques.Length) {
            foreach (var plaque in Plaques) plaque.Value = 0;
            ConnectPlaques();
            Clear();
            return;
        }

        foreach (var (p, i) in plaq.Indexed()) Plaques[i].Value = p;
        ConnectPlaques();
        Clear();
    }

    /// <summary>
    ///     Sets the values of the plaques.
    /// </summary>
    /// <param name="pq">A list of integers representing the values to be set for the plaques.</param>
    public void SetPlaques(params IEnumerable<int> pq) => SetPlaques(pq.ToArray());


    /// <summary>
    ///     Renvoie la solution à la position spécifiée.
    /// </summary>
    /// <param name="no">La position de la solution à renvoyer.</param>
    /// <returns>La solution sous forme de chaîne de caractères, ou <c>null</c> si la position est invalide.</returns>
    public string? Solution(int no = 0) => no >= 0 && no < Solutions.Count ? Solutions[no].ToString() : null;


    /// <summary>
    ///     Valid
    /// </summary>
    public CebStatus Valid() {
        if (Status is not (CebStatus.CompteEstBon or CebStatus.CompteApproche))
            Status = IsSearchValid() && IsPlaquesValid() ? CebStatus.Valide : CebStatus.Invalide;
        return Status;
    }

    #region resolution

    /// <summary>
    ///     Solves the "Compte est bon" problem by exploring all possible combinations of operations on the plaques.
    /// </summary>
    /// <remarks>
    ///     This method uses a stack-based approach to iteratively explore all possible combinations of operations on the
    ///     plaques. It inserts valid solutions into the list of solutions and updates the status accordingly.
    /// </remarks>
    // ReSharper disable PossibleMultipleEnumeration
    private void _solve() {
        void InsertSolution(CebBase sol) {
            var ecart = Abs(Search - sol.Value);
            if (ecart > Ecart) return;
            if (ecart < Ecart) {
                Ecart = ecart;
                _solutions.Clear();
            } else if (_solutions.Contains(sol)) {
                return;
            }

            _solutions.Add(sol);
        }

        var stack = new Stack<IEnumerable<CebBase>>();
        stack.Push(Plaques);

        while (stack.Count > 0) {
            var currentList = stack.Pop();

            foreach (var (g, i) in currentList.Indexed()) {
                InsertSolution(g);

                foreach (var (d, j) in currentList.Indexed().Where((_, j) => j > i)) {
                    var validOperations = CebOperation.ListeOperations
                        .Select(op => new CebOperation(g, op, d))
                        .Where(o => o.Value != 0);

                    foreach (var oper in validOperations)
                        stack.Push(new[] { oper }.Concat(currentList.Where((_, k) => k != i && k != j)));
                }
            }

            Task.Yield();
        }
    }

    // ReSharper enable PossibleMultipleEnumeration
    /// <summary>
    ///     Resolves the current "Compte est bon" problem by finding the best possible solution.
    /// </summary>
    /// <returns>
    ///     The status of the resolution process, indicating whether the solution is exact, approximate, or invalid.
    /// </returns>
    public virtual CebStatus Solve() {
        _solutions.Clear();
        if (Status == CebStatus.Invalide) return Status;
        Status = CebStatus.EnCours;
        Ecart = int.MaxValue;
        _solve();
        Status = Ecart == 0 ? CebStatus.CompteEstBon : CebStatus.CompteApproche;
        _solutions.Sort((p, q) => p.Rank.CompareTo(q.Rank));
        return Status;
    }


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
    public CebStatus Solve(int search, params int[] plq) {
        Status = CebStatus.Indefini;
        Search = search;
        SetPlaques(plq);
        return Solve();
    }

    /// <summary>
    ///     Asynchronously solves the "Compte est bon" problem.
    /// </summary>
    /// <returns>
    ///     A <see cref="CebStatus" /> representing the status of the resolution process.
    /// </returns>
    /// <remarks>
    ///     This method runs the <see cref="Solve" /> method on a separate thread.
    /// </remarks>
    public virtual async ValueTask<CebStatus> SolveAsync() => await Task.Run(Solve);


    /// <summary>
    ///     Résoudre en asynchrone avec paramètres
    /// </summary>
    /// <param name="search"></param>
    /// <param name="plq"></param>
    /// <returns></returns>
    public async ValueTask<CebStatus> SolveAsync(int search, params int[] plq) => await Task.Run(
        () => Solve(search, plq));

    #endregion
}