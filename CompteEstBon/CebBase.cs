// Plage Compte est bon

using System;
using System.Collections.Generic;
using System.Linq;

namespace CompteEstBon {
    [System.Runtime.InteropServices.Guid("F4D942FB-85DF-4391-AE82-9EFE20DDADB0")]
    public abstract class CebBase:  CebDetail {
        /// <summary>
        /// Valeur de la donn�e
        /// </summary>
        public virtual int Value { get; set; } = 0;
        public int Rank { get; protected set; } = 0;
        public abstract bool IsValid { get; }
        public static implicit operator int(CebBase b) => b.Value;
        public override bool Equals(object obj) => (obj is CebBase op && op.Rank == Rank) ?
                            Operations.WithIndex().All(e => e.Item1 == op[e.Item2]) : false;
        public int Compare(CebBase b) => Rank - b.Rank;
        public override int GetHashCode() => base.GetHashCode(); //  HashCode.Combine(this);
        protected int Add(CebBase ceb) {
            if (Rank < 5 && ceb is CebOperation) {
                for (var i = 0; i < ceb.Rank; i++) {
                    this[Rank++] = ceb[i];
                    if (Rank == 5) break;
                }
            }
            return this.Rank;
        }
    }
}