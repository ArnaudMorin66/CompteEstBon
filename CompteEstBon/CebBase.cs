//-----------------------------------------------------------------------
// <copyright file="CebBase.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#pragma warning disable CS1591


using System.Text.Json.Serialization;

using arnaud.morin.outils;

namespace CompteEstBon;

/// <summary>
///     Classe de base pour les plaques et les opérations
/// </summary>
public abstract class CebBase {
	/// <summary>
	///     Initialisations CebBase
	/// </summary>
	protected CebBase() => Operations = new List<string>();

	/// <summary>
	///     Liste des opérations
	/// </summary>
	public List<string> Operations { get; }

	/// <summary>
	///     Valeur de la donnée
	/// </summary>
	[JsonIgnore]
	public virtual int Value { get; set; } = 0;

	[JsonIgnore] public int Rank => Operations.Count;

	/// <summary>
	///     Teste si l'objet est valide
	/// </summary>
	[JsonIgnore]
	public abstract bool IsValid { get; }

	/// <summary>
	///     retourne le détail
	/// </summary>
	//public CebDetail Detail {
	//	get {
	//		var detail = new CebDetail();
	//		foreach (var (operation, i) in Operations.Indexed()) detail[i] = operation;
	//		return detail;
	//	}
	//}

	/// <inheritdoc />
	public override string ToString() => string.Join(", ", Operations);

	/// <summary>
	///     Conversion d'une CebPlaque en int
	/// </summary>
	/// <param name="b"></param>
	public static implicit operator int(CebBase b) => b.Value;

	/// <summary>
	///     Egalité entre deux objets
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	//public override bool Equals(object obj) => (obj is CebBase op && op.Rank == Rank) &&
	//    Operations.Indexed().All(e => string.Compare(e.Item1, op.Operations[e.Item2], StringComparison.Ordinal) == 0);

	//$$public override bool Equals(object obj) => (obj is CebBase op && op.Operations.Count == Operations.Count) &&
	//$$  Operations.Indexed().All(e => string.Compare(e.Item1, op.Operations[e.Item2], StringComparison.Ordinal) == 0);

	//   return false;
	public override bool Equals(object obj) => obj is CebBase op && op.ToString() == ToString();

	/// <inheritdoc />
	public override int GetHashCode() => base.GetHashCode();

	/// <summary>
	///     Conversion base vers détail
	/// </summary>
	/// <param name="b"></param>
	// public static implicit operator CebDetail(CebBase b) => b.Detail;

	public string Op1 => (Operations.Count > 0 ) ? Operations[0]:"";

	/// <summary>
	///     Opération 2
	/// </summary>
	public string Op2=>(Operations.Count > 1 ) ? Operations[1]:"";

	/// <summary>
	///     Opération 2
	/// </summary>
	public string Op3 => (Operations.Count > 2) ? Operations[2] : "";

	/// <summary>
	///     Opération 4
	/// </summary>
	public string Op4 => (Operations.Count > 3) ? Operations[3] : "";

	/// <summary>
	///     Opération 5
	/// </summary>
	public string Op5 => (Operations.Count > 4) ? Operations[4] : "";

}