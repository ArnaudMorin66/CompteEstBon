// Plage Compte est bon

using System;
using System.Collections.Generic;
using System.Linq;

namespace CompteEstBon {
    [System.Runtime.InteropServices.Guid("F4D942FB-85DF-4391-AE82-9EFE20DDADB0")]
    public abstract class CebBase {
        /// <summary>
        /// Valeur de la donn�e
        /// </summary>
        public virtual int Value { get; set; } = 0;

        public int Rank { get; protected set; } = 0;

        public abstract bool IsValid { get; }

        public static implicit operator int(CebBase b) => b.Value;
        public CebDetail Detail { get; } = new CebDetail();

        public IEnumerable<string> Operations => from op in Detail.Operations select op;
        public override bool Equals(object obj) {
            if (GetType() == obj.GetType()) {
                if (obj is CebBase op && op.Value == Value && op.Rank == Rank) {
                    foreach (var (v, i) in Operations.WithIndex()) {
                        if (op.Operations.ElementAt(i) != v) {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        public int Compare(CebBase b) => this.Rank - b.Rank;

        public override int GetHashCode() => base.GetHashCode(); // HashCode.Combine(Detail);
        public bool Add(CebBase ceb) {
            if (Rank >= 5 || ceb is CebPlaque) return false;
            foreach (var oper in ceb.Operations) { 
                Detail[Rank++] = oper;
                if (Rank == 5) break;
            }
            return true;
        }
    }
}