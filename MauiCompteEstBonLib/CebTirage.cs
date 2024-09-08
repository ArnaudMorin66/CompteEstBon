//-----------------------------------------------------------------------
// <copyright file="CebTirage.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#region using

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;


using static System.Math;

#endregion using

namespace CompteEstBon;
#nullable enable
/// <summary>
///     Gestion tirage Compte est bon
/// </summary>
public sealed class CebTirage : INotifyPropertyChanged {
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
	///     Durée de calcul
	/// </summary>
	public TimeSpan Duree { get; private set; }

	/// <summary>
	///     Première solution
	/// </summary>
	public string FirstSolution => Solutions.First().ToString()!;

	/// <summary>
	///     Renvoie le ou les valeurs trouvées
	/// </summary>
	public string Found => string.Join(", ",  Solutions.Select(s => s.Value.ToString()).Distinct().Order());


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
		Duree = TimeSpan.Zero;
		Ecart = null;
		Status = CebStatus.Indefini;
		Valid();
		NotifyPropertyChanged();
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
	/// </summary>
	/// <returns></returns>
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
	/// </summary>
	/// <param name="pq"></param>
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

	/// <param name="sol"></param>
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
	///     boucle de résolution du compte
	/// </summary>
	/// <param name="liste"></param>
	// ReSharper disable PossibleMultipleEnumeration
	private void Resolve(IEnumerable<CebBase> liste) {
		
		foreach (var (g, i) in liste.Indexed()) {
			InsertSolution(g);

			foreach (var (d, j) in liste.Indexed().Where((_, j) => j > i))
			foreach (var oper in CebOperation.ListeOperations
				         .Select(op => new CebOperation(g, op, d))
				         .Where(o => o.Value != 0))
				Resolve(liste.Where((_, k) => k != i && k != j).Append(oper));
		}
	}

	// ReSharper enable PossibleMultipleEnumeration
	/// <summary>
	/// </summary>
	/// <returns></returns>
	public CebStatus Resolve() {
		_solutions.Clear();
		if (Status == CebStatus.Invalide)
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
public static class Outils {
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="self"></param>
	/// <returns></returns>
	public static IEnumerable<(T, int)> Indexed<T>(this IEnumerable<T> self) => self.Select(
		(value, index) => (value, index));

	/// <summary>
	///     Teste si une valeur est entre les 2 valeurs spécifiées (bornes inclues)
	/// </summary>
	/// <typeparam name="T">Type de la valeur</typeparam>
	/// <param name="value">Valeur à tester</param>
	/// <param name="min">Borne inférieure</param>
	/// <param name="max">Borne supérieure</param>
	/// <returns>true si <c>min &lt;= value &lt;= max</c>, sinon false</returns>
	public static bool Between<T>(this T value, T min, T max) where T : IComparable<T> => min.CompareTo(value) <= 0 &&
		value.CompareTo(max) <= 0;

	/// <summary>
	///     Teste si une valeur est strictement entre les bornes spécifiées (bornes non inclues)
	/// </summary>
	/// <typeparam name="T">Type de la valeur</typeparam>
	/// <param name="value">Valeur à tester</param>
	/// <param name="min">Borne inférieure</param>
	/// <param name="max">Borne supérieure</param>
	/// <returns>true si <c>min &lt; value &lt; max</c>, sinon false</returns>
	public static bool StrictlyBetween<T>(this T value, T min, T max) where T : IComparable<T> => min.CompareTo(value) <
		0 &&
		value.CompareTo(max) < 0;

	/// <summary>
	/// </summary>
	/// <param name="nom"></param>
	public static void OpenDocument(this string nom) => Process.Start(
		new ProcessStartInfo { UseShellExecute = true, FileName = nom });
}