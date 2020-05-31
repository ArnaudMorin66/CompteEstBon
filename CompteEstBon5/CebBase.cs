// Plage Compte est bon

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace CompteEstBon {

    [System.Runtime.InteropServices.Guid("F4D942FB-85DF-4391-AE82-9EFE20DDADB0")]
    public abstract class CebBase  {
        public string Op1 => Rank > 0 ? Operations[0] : null;
        public string Op2 => Rank > 1 ? Operations[1] : null;
        public string Op3 => Rank > 2 ? Operations[2] : null;
        public string Op4 => Rank > 3 ? Operations[3] : null;
        public string Op5 => Rank > 4 ? Operations[4] : null;
        public override string ToString() => string.Join(", ", Operations);

        

        public List<string> Operations { get; }
        
        public (int gauche, char op, int droite) Decoup(int i) {
            if (i >= Rank) return   (0, '\0', 0);
            var l = Operations[i].Split();
            if (int.TryParse(l[0], out int g)) return (0, '\0', 0);
            if (this is CebPlaque) return (g, '\0', 0);
            if (!int.TryParse(l[2], out int d)) return (0, '\0', 0);
            return (g, l[1][0], d);
        }

        // public string Op(int i) => Operations[i];
        public void AddOperation(string value) => Operations.Add(value);
        /// <summary>
        /// Valeur de la donn�e
        /// </summary>
        [JsonIgnore]
        public virtual int Value { get; set; } = 0;

        [JsonIgnore]
        public int Rank => Operations.Count;

        [JsonIgnore]
        public abstract bool IsValid { get; }

        public static implicit operator int(CebBase b) => b.Value;

        public override bool Equals(object obj) => (obj is CebBase op && op.Rank == Rank) ?
                            Operations.WithIndex().All(e => e.Item1 == op.Operations[e.Item2]) : false;

        public int Compare(CebBase b) => Rank - b.Rank;

        public override int GetHashCode() => base.GetHashCode();

        protected CebBase() {
            Operations = new List<string>();
        }
    }
}