//-----------------------------------------------------------------------
// <copyright file="CebOperation.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CompteEstBon;

/// <summary>
/// Represents an operation in the "Compte Est Bon" game.
/// This class is responsible for performing arithmetic operations and storing the results.
/// </summary>
public sealed class CebOperation : CebBase {
	/// <summary>
	/// A constant string representing the list of operations available in the "Compte Est Bon" game.
	/// The operations included are addition (+), subtraction (-), multiplication (x), and division (/).
	/// </summary>
	public const string ListeOperations = "x+-/";

	/// <summary>
	///     Constructor opération (g op d)
	/// </summary>
	/// <param name="g">operateur gauche</param>
	/// <param
	///     name="op"> Operation
	/// </param>
	/// <param name="d"> operateur droit</param>
    public CebOperation(CebBase g, char op, CebBase d) {
		if (g.Value < d.Value)
			(g, d) = (d, g);
		Value = op switch {
			'+' => g.Value + d.Value,
			'-' => g.Value - d.Value,
			'x' => g.Value <= 1 || d.Value <= 1 ? 0 : g.Value * d.Value,
			'/' => d.Value <= 1 || g.Value % d.Value != 0 ? 0 : g.Value / d.Value,
			_ => 0
		};
		if (Value != 0)
			AddOperations(g, op, d);
	}


    /// <summary>
    /// Gets a value indicating whether the current operation is valid.
    /// </summary>
    /// <value>
    /// <c>true</c> if the operation is valid; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// An operation is considered valid if its <see cref="Value"/> is greater than zero.
    /// </remarks>
	public override bool IsValid => Value > 0;

	/// <summary>
	/// </summary>
	/// <param name="value"></param>
	private void AddOperation(string value) => Operations.Add(value);

    /// <summary>
    /// </summary>
    /// <param name="ceb"></param>

private void AddOperation(CebBase ceb) {
		if (ceb is CebOperation)
			Operations.AddRange(ceb.Operations);
	}

	/// <summary>
	/// Adds the operations performed between the left operand, operator, and right operand to the list of operations.
	/// </summary>
	/// <param name="gauche">The left operand of the operation.</param>
	/// <param name="op">The operator used in the operation.</param>
	/// <param name="droite">The right operand of the operation.</param>
	private void AddOperations(CebBase gauche, char op, CebBase droite) {
		AddOperation(gauche);
		AddOperation(droite);
		AddOperation($"{gauche.Value} {op} {droite.Value} = {Value}");
	}
}