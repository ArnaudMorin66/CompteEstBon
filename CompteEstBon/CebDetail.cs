//-----------------------------------------------------------------------
// <copyright file="CebDetail.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#pragma warning disable CS1591

using System.Text.Json.Serialization;

namespace CompteEstBon; 

public record CebDetail {
    public string Op1 { get; set; } = null;

    public string Op2 { get; set; } = null;

    public string Op3 { get; set; } = null;

    public string Op4 { get; set; } = null;

    public string Op5 { get; set; } = null;

    [JsonIgnore]

    public IEnumerable<string> Operations {
        get {
            yield return Op1;

            if (Op2 is null)
                yield break;
            yield return Op2;


            if (Op3 is null)
                yield break;
            yield return Op3;

            if (Op4 is null)
                yield break;
            yield return Op4;

            if (Op5 is null)
                yield break;
            yield return Op5;
        }
    }

    public string Op(int i) => GetType().GetProperty($"Op{i + 1}")!.GetValue(this) as string;

    public void SetOp(int i, string value) => GetType().GetProperty($"Op{i + 1}")!.SetValue(this, value);

    public override string ToString() => string.Join(", ", Operations);

    public string this[int i] {
        get => Op(i);
        set => SetOp(i, value);
    }
}