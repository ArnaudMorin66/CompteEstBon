using System.Collections.Generic;

namespace CompteEstBon {
    [System.Runtime.InteropServices.Guid("F4D942FB-85DF-4391-AE82-9EFE20DDADB0")]
    public abstract class CebBase {
        /// <summary>
        /// Valeur de la donn�e
        /// </summary>
        public virtual int Value { get; set; } = 0;

        public abstract int Rank { get; }
        public string this[int index] => index < Operations.Count ? Operations[index] : null;

        public abstract List<string> Operations { get; }

        public abstract bool IsValid { get; }

        public abstract CebDetail Detail { get; }

        //  User-defined conversion from double to Digit
        public static implicit operator int(CebBase b) => b.Value;
    }
    
}