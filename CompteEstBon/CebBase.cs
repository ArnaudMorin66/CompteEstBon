// Plage Compte est bon

using System.Collections.Generic;

namespace CompteEstBon {
    [System.Runtime.InteropServices.Guid("F4D942FB-85DF-4391-AE82-9EFE20DDADB0")]
    public abstract class CebBase {
        /// <summary>
        /// Valeur de la donn�e
        /// </summary>
        public virtual int Value { get; set; } = 0;

        public int Rank { get; set; } = 0;
        public List<string> Operations { get; } = new List<string>();

        public abstract bool IsValid { get; }

        // public abstract CebDetail Detail { get; }

        public static implicit operator int(CebBase b) => b.Value;
    }

}