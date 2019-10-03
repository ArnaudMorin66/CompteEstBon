// Plage Compte est bon

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CompteEstBon {

    [System.Runtime.InteropServices.Guid("A21F3DEC-8531-4F59-AF11-863BEF5ED340")]
    public sealed class CebPlaque : CebBase, INotifyPropertyChanged {

        public static readonly int[] ListePlaques = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            25, 50, 75, 100
        };

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Plaque
        /// </summary>
        /// <param name="value">
        /// </param>
        /// <param name="handler"></param>
        public CebPlaque(int value, PropertyChangedEventHandler /* EventHandler<int>*/ handler = null) {
            Value = value;
            this.Operations.Add(value.ToString());
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
                this.Operations[0] = value.ToString();
                NotifyPropertyChanged();
            }
        }

        public int Value2 {
            get => Value;
            set {
                if (Value == value) return;
                base.Value = value;
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
        /// <summary>
        /// D�termine si le <see cref="T:System.Object" /> sp�cifi� est �gal au
        /// <see cref="T:System.Object" /> actif.
        /// </summary>
        /// <returns>
        /// true si le <see cref="T:System.Object" /> sp�cifi� est �gal au
        /// <see cref="T:System.Object" /> actif�; sinon, false.
        /// </returns>
        /// <param name="obj">
        /// <see cref="T:System.Object" /> � comparer au <see cref="T:System.Object" /> actif.
        /// </param>
        /// <filterpriority>
        /// 2
        /// </filterpriority>
        public override bool Equals(object obj) => obj is CebPlaque p && p.Value == Value;

        public override int GetHashCode() => 391 + Value.GetHashCode();

        // public override CebDetail Detail => new CebDetail(this);

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}