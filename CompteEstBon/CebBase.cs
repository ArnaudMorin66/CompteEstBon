//Arnaud Morin

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CompteEstBon {

    [System.Runtime.InteropServices.Guid("F4D942FB-85DF-4391-AE82-9EFE20DDADB0")]
    public abstract class CebBase {
        /// <summary>
        /// Valeur de la donnée
        /// </summary>
        public virtual int Value { get; set; } = 0;

        /// <summary>
        /// Nb de plaques
        /// </summary>
        public abstract int Rank { get; }

        public string this[int index] => index < Operations.Count ? Operations[index] : null;

        public virtual List<string> Operations { get; set; }

        public string[] ToArray() => Operations.ToArray();

        public virtual bool IsValid => Value > 0;

        public override int GetHashCode() {
            unchecked {
                int result = 17;
                result = result * 23 + Value.GetHashCode();
                result = result * 23 + Rank.GetHashCode();
                return result;
            }
        }

        public static explicit operator int(CebBase p) => p.Value;

        public override string ToString() => Value.ToString();

        public CebOperationDetail ToCebOperationDetail() {
            var elt = new CebOperationDetail();

            foreach (var fld in elt.GetType().GetProperties()
                .Where(p => p.Name.StartsWith("op"))
                .Select((q, i) => new { instance = q, value = i >= Operations.Count ? "" : Operations[i] })) {
                fld.instance.SetValue(elt, fld.value);
            }
            return elt;
        }

        /// <summary>
        /// Détermine si le <see cref="T:System.Object" /> spécifié est égal au
        /// <see cref="T:System.Object" /> actif.
        /// </summary>
        /// <returns>
        /// true si le <see cref="T:System.Object" /> spécifié est égal au
        /// <see cref="T:System.Object" /> actif ; sinon, false.
        /// </returns>
        /// <param name="obj">
        /// <see cref="T:System.Object" /> à comparer au <see cref="T:System.Object" /> actif.
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
    }
}