//-----------------------------------------------------------------------
// <copyright file="CebOperation.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CompteEstBon;

/// <inheritdoc/>
/// /// /// ///
/// <summary>
/// Classe op�ration
/// </summary>
public sealed class CebOperation : CebBase {
    /// <summary>
    ///
    /// </summary>
    public const string ListeOperations = "x+-/";

    /// <summary>
    /// Constructor opération (g op d)
    /// </summary>
    /// <param name="g"></param>
    /// <param
    ///     name="op">
    ///
    /// </param>
    /// <param name="d"></param>
    public CebOperation(CebBase g, char op, CebBase d) {
        if (g.Value < d.Value)
            (g, d) = (d, g);
        Value = op switch
        {
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
    ///
    /// </summary>
    public override bool IsValid => Value > 0;

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    private void AddOperation(string value) => Operations.Add(value);

    /// <summary>
    ///
    /// </summary>
    /// <param name="ceb"></param>
    private void AddOperation(CebBase ceb) {
        if (ceb is CebOperation)
            Operations.AddRange(ceb.Operations);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="gauche"></param>
    /// <param name="op"></param>
    /// <param name="droite"></param>
    private void AddOperations(CebBase gauche, char op, CebBase droite) {
        AddOperation(gauche);
        AddOperation(droite);
        AddOperation($"{gauche.Value} {op} {droite.Value} = {Value}");
    }
}