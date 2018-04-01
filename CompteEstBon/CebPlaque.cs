// Plage Compte est bon

using System.Linq;

namespace CompteEstBon
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    [System.Runtime.InteropServices.Guid("A21F3DEC-8531-4F59-AF11-863BEF5ED340")]
    public sealed class CebPlaque : CebBase
    {
        public static readonly int[] ListePlaques =
        {
            1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 25,
            25, 50, 75, 100
        };

        // public event EventHandler ValueChanged;

        /// <summary>
        /// </summary>
        /// <param name="tirage"></param>
        /// <param name="value"></param>
        public CebPlaque(CebTirage tirage, int value)
        {
            Tirage = tirage;
            Value = value;
        }
        public string  Text
        {
            get => Value.ToString();
            set { Value = int.TryParse(value, out int res) ? res : 0; }
        }
        public override int Value
        {
            get => base.Value;
            set
            {
                base.Value = value;
                Tirage.Clear();
            }
        }

        /// <summary>
        /// </summary>
        public override int Rank => 0;

        public override bool IsValid => ListePlaques.Contains(Value);
        public override string[] Operations => new[] {Text};

        public CebTirage Tirage { get; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator int(CebPlaque p)
        {
            return p.Value;
        }
        //}
        //    ValueChanged?.Invoke(this,null);
        //{

        //private void OnValueChanged()
    }
}