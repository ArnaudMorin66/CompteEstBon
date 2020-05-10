// Plage Compte est bon

using System.Linq;
using System.Text.Json.Serialization;

namespace CompteEstBon {

    [System.Runtime.InteropServices.Guid("F4D942FB-85DF-4391-AE82-9EFE20DDADB0")]
    public abstract class CebBase : CebDetail {

        /// <summary>
        /// Valeur de la donn�e
        /// </summary>
        [JsonIgnore]
        public virtual int Value { get; set; } = 0;

        [JsonIgnore]
        public int Rank { get; protected set; } = 0;

        [JsonIgnore]
        public abstract bool IsValid { get; }

        public static implicit operator int(CebBase b) => b.Value;

        public override bool Equals(object obj) => (obj is CebBase op && op.Rank == Rank) ?
                            Operations.WithIndex().All(e => e.Item1 == op[e.Item2]) : false;

        public int Compare(CebBase b) => Rank - b.Rank;

        public override int GetHashCode() => base.GetHashCode();
    }
}