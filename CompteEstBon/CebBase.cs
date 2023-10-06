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
/// Classe de base pour les plaques et les opérations
/// </summary>
public abstract class CebBase {
    /// <inheritdoc/>
    public override string ToString() => string.Join(", ", Operations);

    /// <summary>
    /// Liste des opérations
    /// </summary>
    public List<string> Operations { get; }

    /// <summary>
    /// Valeur de la donnée
    /// </summary>
    [JsonIgnore]
    public virtual int Value { get; set; } = 0;
    /// <summary>
    /// Conversion d'une CebPlaque en int
    /// </summary>
    /// <param name="b"></param>
    public static implicit operator int(CebBase b) => b.Value;

    [JsonIgnore]
    public int Rank => Operations.Count;

    /// <summary>
    /// Teste si l'objet est valide
    /// </summary>
    [JsonIgnore]
    public abstract bool IsValid { get; }

    /// <summary>
    /// Egalité entre deux objets
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    //public override bool Equals(object obj) => (obj is CebBase op && op.Rank == Rank) &&
    //    Operations.Indexed().All(e => string.Compare(e.Item1, op.Operations[e.Item2], StringComparison.Ordinal) == 0);
    public override bool Equals(object obj) {
        if (object.ReferenceEquals(this, obj)) return true;
        return obj is CebBase op && Operations.Indexed().All(e =>
            string.Compare(e.Item1, op.Operations[e.Item2], StringComparison.Ordinal) == 0);
    }
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();

    /// <summary>
    /// Initialisations CebBase
    /// </summary>
    protected CebBase() => Operations = new List<string>();

    /// <summary>
    /// retourne le détail
    /// </summary>
    public CebDetail Detail {
        get {
            CebDetail detail = new CebDetail();
            foreach (var (operation, i) in Operations.Indexed()) detail[i] = operation;
            return detail;
        }
    }

    /// <summary>
    /// Conversion base vers détail
    /// </summary>
    /// <param name="b"></param>
    public static implicit operator CebDetail(CebBase b) => b.Detail;
}
