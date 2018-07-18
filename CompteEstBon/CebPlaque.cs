// Plage Compte est bon

using System.Collections.Generic;
using System.Linq;

namespace CompteEstBon {

    [System.Runtime.InteropServices.Guid("A21F3DEC-8531-4F59-AF11-863BEF5ED340")]
    public sealed class CebPlaque : CebBase {

        public event System.EventHandler<int> ValueEvent;

        public static readonly int[] ListePlaques = {
            1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 25,
            25, 50, 75, 100
        };

        /// <summary>
        /// Plaque
        /// </summary>
        /// <param name="tirage">
        /// </param>
        /// <param name="value">
        /// </param>
        /// <param name="handler"></param>
        public CebPlaque(int value, System.EventHandler<int> handler = null) {
            Value = value;
            if (handler != null)
                ValueEvent += handler;
        }

        public string Text {
            get => Value.ToString();
            set { Value = int.TryParse(value, out int res) ? res : 0; }
        }

        public override int Value {
            get => base.Value;
            set {
                if (base.Value == value) return;
                base.Value = value;
                ValueEvent?.Invoke(this, value);
            }
        }

        /// <summary>
        /// Rang
        /// </summary>
        public override int Rank => 1;

        public override bool IsValid => ListePlaques.Contains(Value);
        public override List<string> Operations => new List<string> { Text };

        /// <summary>
        /// covertion
        /// </summary>
        /// <returns>
        /// </returns>
        public override string ToString() => Value.ToString();

        public static implicit operator int(CebPlaque p) => p.Value;
    }
}