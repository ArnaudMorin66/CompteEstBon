//-----------------------------------------------------------------------
// <copyright file="CebBase.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#pragma warning disable CS1591


using System.Text.Json.Serialization;

using arnaud.morin.outils;
// ReSharper disable UnusedMember.Global

namespace CompteEstBon;

/// <summary>
/// Represents the base class for plaques and operations in the "Compte Est Bon" game.
/// This abstract class provides common properties and methods for derived classes.
/// </summary>
public abstract class CebBase {
	/// <summary>
	/// Initializes a new instance of the <see cref="CebBase"/> class.
	/// </summary>
	/// <remarks>
	/// This constructor initializes the <see cref="Operations"/> property to an empty list.
	/// </remarks>
	protected CebBase() => Operations = [];

	/// <summary>
	///     Liste des opérations
	/// </summary>
	public List<string> Operations { get; }

	/// <summary>
	/// Gets or sets the value associated with this instance of <see cref="CebBase"/>.
	/// </summary>
	/// <remarks>
	/// This property represents the numerical value of the current instance. 
	/// It is used to determine the result of operations or the value of plaques in the "Compte Est Bon" game.
	/// </remarks>
	/// <value>
	/// The integer value of the current instance. The default value is 0.
	/// </value>
	[JsonIgnore]
	public virtual int Value { get; set; } = 0;

	/// <summary>
	/// Gets the rank of the current instance based on the number of operations.
	/// </summary>
	/// <remarks>
	/// The rank is determined by the count of operations in the <see cref="Operations"/> list.
	/// </remarks>
	[JsonIgnore] public int Rank => Operations.Count;

	/// <summary>
	/// Gets a value indicating whether the current instance is valid.
	/// </summary>
	/// <value>
	/// <c>true</c> if the current instance is valid; otherwise, <c>false</c>.
	/// </value>
	[JsonIgnore]
	public abstract bool IsValid { get; }

	

	/// <summary>
	/// Returns a string that represents the current object.
	/// </summary>
	/// <returns>
	/// A string that represents the current object, consisting of a comma-separated list of operations.
	/// </returns>
	public override string ToString() => string.Join(", ", Operations);

	/// <summary>
	/// Converts a <see cref="CebBase"/> instance to an integer, returning the value of the instance.
	/// </summary>
	/// <param name="b">The <see cref="CebBase"/> instance to convert.</param>
	/// <returns>The integer value of the <see cref="CebBase"/> instance.</returns>
	public static implicit operator int(CebBase b) => b.Value;

	public override bool Equals(object obj) => obj is CebBase op && op.ToString() == ToString();

	/// <summary>
	/// Serves as the default hash function.
	/// </summary>
	/// <returns>
	/// A hash code for the current object.
	/// </returns>
	public override int GetHashCode() => base.GetHashCode();

	/// <summary>
	///     Conversion base vers détail
	/// </summary>
	/// <param name="b"></param>
	// public static implicit operator CebDetail(CebBase b) => b.Detail;

	public string? Op1 => (Operations.Count > 0 ) ? Operations[0]:null;

	/// <summary>
	///     Opération 2
	/// </summary>
	public string? Op2=>(Operations.Count > 1 ) ? Operations[1]:null;

	/// <summary>
	/// Gets the third operation in the list of operations, if it exists.
	/// </summary>
	/// <value>
	/// A string representing the third operation if it exists; otherwise, <c>null</c>.
	/// </value>
	public string? Op3 => (Operations.Count > 2) ? Operations[2] : null;

	/// <summary>
	/// Gets the fourth operation in the list of operations, if it exists.
	/// </summary>
	/// <value>
	/// The fourth operation as a string if the list contains at least four operations; otherwise, <c>null</c>.
	/// </value>
	public string? Op4 => (Operations.Count > 3) ? Operations[3] : null;

	/// <summary>
	/// Gets the fifth operation in the list of operations, if it exists.
	/// </summary>
	/// <value>
	/// The fifth operation as a string if the list contains at least five elements; otherwise, <c>null</c>.
	/// </value>
	public string? Op5 => (Operations.Count > 4) ? Operations[4] : null;

}