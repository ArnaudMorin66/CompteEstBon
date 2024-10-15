//-----------------------------------------------------------------------
// <copyright file="CebTirage.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#region using

using System.ComponentModel;
using System.Runtime.CompilerServices;

using arnaud.morin.outils;

using static System.Math;

#endregion using

namespace CompteEstBon;
#nullable enable
/// <summary>
///     Gestion tirage Compte est bon
/// </summary>
public sealed class CebTirage:  INotifyPropertyChanged {
	private static readonly Random Rnd = System.Random.Shared;

	private readonly List<CebBase> _solutions = [];

	private int _search;

	/// <summary>
	///     <param name="n">Nombre de plaques du tirage</param>
	/// </summary>
	public CebTirage(int n = 6) {
		Plaques = new CebPlaque[n];
		Random();
	}

	/// <summary>
	///     Constructeur Tirage du Compte est bon
	/// </summary>
	/// <param name="n"></param>
	/// <param name="search"></param>
	/// <param name="plaques"></param>
	public CebTirage(int n, int search, params int[] plaques) : this(n) {
		if (plaques.Length != 0)
			SetPlaques(plaques);
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
	public int? Ecart { get; private set; }
    

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
		Search = Search, Plaques = Plaques.Select(p => p.Value).ToArray(), Status = Status.ToString(), Ecart = Ecart,
		Solutions = Solutions.Select(p => p.ToString()).ToList(), Found = Found
	};

	/// <summary>
	///     nombre à chercher
	/// </summary>
	public int Search {
		get => _search;
		set {
			if (value == _search)
				return;
			_search = value;
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
	public CebStatus Status { get; private set; } = CebStatus.Indefini;

	/// <summary>
	///     Evènement de changement
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	///     Retourne si la plaque est valide
	/// </summary>
	/// <returns></returns>
	private bool IsPlaquesValid() => Plaques.All(
		p => p.IsValid && Plaques.Count(q => q.Value == p.Value) <=
			CebPlaque.ListePlaques.Count(i => i == p.Value));

	/// <summary>
	///     Valid the search value
	/// </summary>
	/// <returns>
	/// </returns>
	private bool IsSearchValid() => _search is >= 100 and <= 999;

	/// <summary>
	///     Notifier changement propriété
	/// </summary>
	/// <param name="propertyName"></param>
	// ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
	private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(
		this,
		new PropertyChangedEventArgs(propertyName));

	/// <summary>
	///     Si la plaque est modifiée
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="args"></param>
	private void PlaqueUpdated(object sender, PropertyChangedEventArgs args) {
		if (Status is not (CebStatus.EnCours or CebStatus.Indefini))
			Clear();
	}


    /// <summary>
    ///     Remise à zéro des données
    /// </summary>
    /// <returns></returns>
    public CebStatus Clear() {
        _solutions.Clear();
        Ecart = null;
        Status = CebStatus.Indefini;
        Valid();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Clear"));
        return Status;
    }


    /// <summary>
    ///     Remise à zéro asynchrone
    /// </summary>
    /// <returns></returns>
    public async ValueTask<CebStatus> ClearAsync() => await Task.Run(Clear);

	/// <summary>
	///     Select the value and the plaque's list
	/// </summary>
	public CebStatus Random() {
		Status = CebStatus.Indefini;
		List<int> liste = [..CebPlaque.ListePlaques];

		for (var i = 0; i < Plaques.Length; i++) {
			var n = Rnd.Next(0, liste.Count);
			Plaques[i] = new CebPlaque(liste[n]);
			Plaques[i].PropertyChanged += PlaqueUpdated!;
			liste.RemoveAt(n);
		}

		_search = Rnd.Next(100, 1000);
		return Clear();
	}

	/// <summary>
	/// Asynchronously selects the value and the plaque's list.
	/// </summary>
	/// <returns>
	/// A <see cref="CebStatus"/> representing the status of the operation.
	/// </returns>
	/// <remarks>
	/// This method runs the <see cref="Random"/> method on a separate thread.
	/// </remarks>
	public async ValueTask<CebStatus> RandomAsync() => await Task.Run(Random);

	/// <summary>
	///     resolution
	/// </summary>
	/// <returns>
	/// </returns>
	public void SetPlaques(params int[] plaq) {
		Status = CebStatus.Indefini;
		if (plaq.Length != Plaques.Length) {
			foreach (var plaque in Plaques)
				plaque.Value = 0;
			Clear();
			return;
		}

		foreach (var (p, i) in plaq.Indexed()) Plaques[i].Value = p;
		Clear();
	}

	/// <summary>
	/// Sets the values of the plaques.
	/// </summary>
	/// <param name="pq">A list of integers representing the values to be set for the plaques.</param>
	public void SetPlaques(IList<int> pq) => SetPlaques(pq.ToArray());

	/// <summary>
	/// </summary>
	/// <param name="no"></param>
	/// <returns></returns>
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
	/// Inserts a solution into the list of solutions if it meets certain criteria.
	/// </summary>
	/// <param name="sol">The solution to be inserted.</param>
	private void InsertSolution(CebBase sol) {
		var ecart = Abs(_search - sol.Value);
		switch (ecart - Ecart) {
			case > 0:
				return;
			case 0: {
				if (_solutions.Contains(sol))
					return;
				break;
			}
			case < 0: {
				Ecart = ecart;
				_solutions.Clear();
				break;
			}
		}
		
		_solutions.Add(sol);
	}


    /// <summary>
    ///     Resolves the "Compte Est Bon" game by iterating through possible solutions.
    /// </summary>
    /// <param name="liste">
    ///     The initial list of <see cref="CebBase"/> elements to start the resolution process.
    /// </param>
    /// <remarks>
    ///     This method uses a stack-based approach to explore all possible combinations of operations
    ///     and values to find valid solutions. It inserts each solution found and continues to explore
    ///     further combinations until all possibilities are exhausted.
    /// </remarks>
    // ReSharper disable PossibleMultipleEnumeration
    
    private void Resolve(IEnumerable<CebBase> liste) {
        var stack = new Stack<IEnumerable<CebBase>>();
        stack.Push(liste);

        while (stack.Count > 0) {
            var currentList = stack.Pop();

            foreach (var (g, i) in currentList.Indexed()) {
                InsertSolution(g);

                foreach (var (d, j) in currentList.Indexed().Where((_, j) => j > i)) {
                    var validOperations = CebOperation.ListeOperations
                        .Select(op => new CebOperation(g, op, d))
                        .Where(o => o.Value != 0);

                    foreach (var oper in validOperations) {
                        stack.Push(NextList(currentList, oper, i, j));
                    }
                }
            }
        }
    }

    private IEnumerable<CebBase> NextList(IEnumerable<CebBase> current, CebBase operation, int i, int j) {
        foreach (var (item, k) in current.Indexed()) {
            if (k != i && k != j) {
                yield return item;
            }
        }

        yield return operation;
    }

    // ReSharper enable PossibleMultipleEnumeration
    /// <summary>
    ///     Resolves the current "Compte est bon" problem by finding the best possible solution.
    /// </summary>
    /// <returns>
    ///     The status of the resolution process, indicating whether the solution is exact, approximate, or invalid.
    /// </returns>
    public CebStatus Resolve() {
		_solutions.Clear();
		if (Status == CebStatus.Invalide)
			return Status;
		Status = CebStatus.EnCours;
		Ecart = int.MaxValue;
		Resolve(Plaques);
		Status = Ecart == 0 ? CebStatus.CompteEstBon : CebStatus.CompteApproche;
		_solutions.Sort((p, q) => p.Rank.CompareTo(q.Rank));
		NotifyPropertyChanged(nameof(Solutions));
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
	public CebStatus Resolve(int search, params int[] plq) {
		Status = CebStatus.Indefini;
		_search = search;
		SetPlaques(plq);
		return Resolve();
	}

	/// <summary>
	/// </summary>
	/// <returns></returns>
	public async ValueTask<CebStatus> ResolveAsync() => await Task.Run(Resolve);

	/// <summary>
	///     Résoudre en asynchrone avec paramètres
	/// </summary>
	/// <param name="search"></param>
	/// <param name="plq"></param>
	/// <returns></returns>
	public async ValueTask<CebStatus> ResolveAsync(int search, params int[] plq) => await Task.Run(
		() => Resolve(search, plq));

	#endregion
}