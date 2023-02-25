//-----------------------------------------------------------------------
// <copyright file="CebDetail.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#pragma warning disable CS1591

using System.Text.Json.Serialization;

namespace CompteEstBon;

/// <summary>
/// Renvoi la liste des operations d'une solution sous forme de variables
/// </summary>
public record CebDetail {
    /// <summary>
    ///
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public string this[int i] { get => Op(i); set => SetOp(i, value); }


    public override string ToString() => string.Join(", ", Operations);

    /// <summary>
    /// Opération 1
    /// </summary>
    public string Op1 { get; set; } = null;

    /// <summary>
    /// Opération 2
    /// </summary>
    public string Op2 { get; set; } = null;

    /// <summary>
    /// Opération 2
    /// </summary>
    public string Op3 { get; set; } = null;

    /// <summary>
    /// Opération 4
    /// </summary>
    public string Op4 { get; set; } = null;

    /// <summary>
    /// Opération 5
    /// </summary>
    public string Op5 { get; set; } = null;

    /// <summary>
    /// Operations énumérable
    /// </summary>
    [JsonIgnore]

    public IEnumerable<string> Operations {
        get {
            for(int i = 0; i < 5; i++) {
                if(Op(i) is null)
                    yield break;
                yield return Op(i);
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public string Op(int i) => GetType().GetProperty($"Op{i + 1}")!.GetValue(this) as string;

    /// <summary>
    /// Met à jours la valeur d'un champ
    /// </summary>
    /// <param name="i"></param>
    /// <param name="value"></param>
    public void SetOp(int i, string value) => GetType().GetProperty($"Op{i + 1}")!.SetValue(this, value);
}