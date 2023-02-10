//-----------------------------------------------------------------------
// <copyright file="CebPlaque.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// Plage Compte est bon

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CompteEstBon;
/// <summary>
/// 
/// </summary>
public sealed class CebPlaque : CebBase, INotifyPropertyChanged {
    public static readonly int[] ListePlaques = {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 25, 50, 75, 100,
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 25
    };
    /// <summary>
    /// 
    /// </summary>
    public static readonly IEnumerable<int> DistinctPlaques = ListePlaques.Distinct();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    public CebPlaque(int v = 0) {
        Operations.Add(v.ToString());
        Value = v;
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    public string Text {
        get => Value.ToString();
        set => Value = int.TryParse(value, out var res) ? res : 0;
    }
    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    public override int Value {
        get => base.Value;
        set {
            if (base.Value == value)
                return;
            base.Value = value;
            Operations[0] = value.ToString();
            OnPropertyChanged();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore] public override bool IsValid => DistinctPlaques.Contains(Value);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p"></param>
    public static implicit operator int(CebPlaque p) => p.Value;
    /// <summary>
    /// 
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}