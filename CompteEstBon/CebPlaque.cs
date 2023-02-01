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

public sealed class CebPlaque : CebBase, INotifyPropertyChanged {
    public static readonly int[] AllPlaques = {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 25, 50, 75, 100,
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 25
    };

    public static readonly IEnumerable<int> AnyPlaques = new SortedSet<int>(AllPlaques);


    public CebPlaque(int v = 0) {
        Operations.Add(v.ToString());
        Value = v;
    }


    [JsonIgnore]
    public string Text {
        get => Value.ToString();
        set => Value = int.TryParse(value, out var res) ? res : 0;
    }

    [JsonIgnore]
    public override int Value {
        get => base.Value;
        set {
            if (base.Value == value)
                return;
            base.Value = value;
            Operations[0] = value.ToString();
            OnPropertyChanged(nameof(Value));
        }
    }

    [JsonIgnore] public override bool IsValid => AnyPlaques.Contains(Value);

    public static implicit operator int(CebPlaque p) => p.Value;

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}