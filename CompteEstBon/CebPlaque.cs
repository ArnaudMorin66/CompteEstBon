// Plage Compte est bon

using System;
using System.Collections.Generic;
using System.Linq;

namespace CompteEstBon {

    [System.Runtime.InteropServices.Guid("A21F3DEC-8531-4F59-AF11-863BEF5ED340")]
    public sealed class CebPlaque : CebBase {

        public static readonly int[] ListePlaques = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            25, 50, 75, 100, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,25
        };

        public event EventHandler<int> ValueChanged;

        /// <summary>
        /// Plaque
        /// </summary>
        /// <param name="value">
        /// </param>
        /// <param name="handler"></param>
        public CebPlaque(int value, EventHandler<int> handler = null) {
            Value = value;
            if (handler != null)
                ValueChanged += handler;
        }

        public string Text {
            get => Value.ToString();
            set => Value = int.TryParse(value, out var res) ? res : 0;
        }

        public override List<string> Operations => new List<string> {
                    ToString()
                };

        public override int Value {
            get => base.Value;
            set {
                if (base.Value == value) return;
                base.Value = value;
                OnValueChanged(value);
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
        public override int Rank => 0;

        public override bool IsValid => ListePlaques.Contains(Value);

        /// <summary>
        /// covertion
        /// </summary>
        /// <returns>
        /// </returns>
        public override string ToString() {
            return Value.ToString();
        }

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
        public override bool Equals(object obj) {
            return obj is CebPlaque p ? p.Value == Value : false;
        }

        public override int GetHashCode() {
            return 391 + Value.GetHashCode();
        }

        public override CebDetail ToCebDetail => new CebDetail { op1 = ToString() };

        private void OnValueChanged(int e) {
            ValueChanged?.Invoke(this, e);
        }
    }
}