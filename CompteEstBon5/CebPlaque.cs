// Plage Compte est bon

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

#pragma warning disable IDE1006 // Styles d'affectation de noms

namespace CompteEstBon {
#pragma warning restore IDE1006 // Styles d'affectation de noms

    [System.Runtime.InteropServices.Guid("A21F3DEC-8531-4F59-AF11-863BEF5ED340")]
    public sealed class CebPlaque : CebBase, INotifyPropertyChanged {

        public static readonly int[] AllPlaques = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 25, 50, 75, 100,
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 25
        };

        public static IEnumerable<int> AnyPlaques = new SortedSet<int>(AllPlaques);

        public event PropertyChangedEventHandler PropertyChanged;

        public CebPlaque(int value, PropertyChangedEventHandler /* EventHandler<int>*/ handler = null) {
            
            Value = value;
            AddOperation(value.ToString());
            if (handler != null)
                PropertyChanged += handler;
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
                if (base.Value == value) return;
                base.Value = value;
                Operations[0] = value.ToString();
                NotifyPropertyChanged();
            }
        }

        [JsonIgnore]
        public override bool IsValid => AnyPlaques.Contains(Value);

        public static implicit operator int(CebPlaque p) => p.Value;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}