// Plage Compte est bon

using System.Collections.Generic;
using System.Linq;

namespace CompteEstBon {
    public delegate void ValueChange();

    [System.Runtime.InteropServices.Guid("A21F3DEC-8531-4F59-AF11-863BEF5ED340")]
    public sealed class CebPlaque : CebBase {
        public static event ValueChange ValueEvent;
        public static readonly int[] ListePlaques = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            25, 50, 75, 100, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,25
        };

        /// <summary>
        /// Plaque
        /// </summary>
        /// <param name="tirage">
        /// </param>
        /// <param name="value">
        /// </param>
        /// <param name="handler"></param>
        public CebPlaque(int value) {
            Value = value;
        }

        public string Text {
            get => Value.ToString();
            set { Value = int.TryParse(value, out int res) ? res : 0; }
        }

        public override List<string> Operations {
            get {
                return new List<string> {
                    ToString()
                };
            }
        }

        public override int Value {
            // get => base.Value;
            set {
                if (base.Value == value) return;
                base.Value = value;
                ValueEvent?.Invoke();
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
        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            if (GetType() != obj.GetType()) {
                return false;
            }

            return ((CebBase)obj).Value == Value;
        }

        public override int GetHashCode() {
            unchecked {
                int result = 17;
                result = result * 23 + Value.GetHashCode();
                result = result * 23 + Rank.GetHashCode();
                return result;
            }
        }
        public override CebDetail ToCebDetail() {
            return new CebDetail { op1 = ToString() };
        }
    }
}