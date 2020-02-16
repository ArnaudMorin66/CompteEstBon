// Plage Compte est bon

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

#pragma warning disable IDE1006 // Styles d'affectation de noms
namespace CompteEstBon {
#pragma warning restore IDE1006 // Styles d'affectation de noms

    [System.Runtime.InteropServices.Guid("A21F3DEC-8531-4F59-AF11-863BEF5ED340")]
    public sealed class CebPlaque : CebBase, INotifyPropertyChanged {

        public static readonly int[] ListePlaques = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            25, 50, 75, 100, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            25
        };
        public static IEnumerable<int> ListePlaquesUniques = new SortedSet<int>(CebPlaque.ListePlaques);

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Plaque
        /// </summary>
        /// <param name="value">
        /// </param>
        /// <param name="handler"></param>
        public CebPlaque(int value, PropertyChangedEventHandler /* EventHandler<int>*/ handler = null) {
            Rank = 0;
            Value = value;
            Detail.Op1 = value.ToString();
            if (handler != null)
                PropertyChanged += handler;
        }

        public string Text {
            get => Value.ToString();
            set => Value = int.TryParse(value, out var res) ? res : 0;
        }


        public override int Value {
            get => base.Value;
            set {
                if (base.Value == value) return;
                base.Value = value;
                Detail.Op1 = value.ToString();
                NotifyPropertyChanged();
            }
        }


        /// <summary>
        /// Rang
        /// </summary>

        public override bool IsValid => ListePlaques.Contains(Value);

        /// <summary>
        /// covertion
        /// </summary>
        /// <returns>
        /// </returns>
        public override string ToString() => Value.ToString();

        public static implicit operator int(CebPlaque p) => p.Value;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}