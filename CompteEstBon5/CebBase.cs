// Plage Compte est bon

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace CompteEstBon {

    [System.Runtime.InteropServices.Guid("F4D942FB-85DF-4391-AE82-9EFE20DDADB0")]
    public abstract class CebBase  {
        public override string ToString() => string.Join(", ", Operations);
        

        public List<string> Operations { get; private set; }
        
        // public string[] ToArray() => Operations.ToArray();
        public (int gauche, char op, int droite) Decoup(int i) {
            if (i >= Rank) return   (0, '\0', 0);
            var l = Operations[i].Split();
            if (int.TryParse(l[0], out int g)) return (0, '\0', 0);
            if (this is CebPlaque) return (g, '\0', 0);
            return !int.TryParse(l[2], out var d) ? (0, '\0', 0) : (g, l[1][0], d);
        }

        
        /// <summary>
        /// Valeur de la donn�e
        /// </summary>
        [JsonIgnore]
        public virtual int Value { get; set; } = 0;

        [JsonIgnore]
        public int Rank =>  Operations.Count;
        
        [JsonIgnore]
        public abstract bool IsValid { get; }

        public static implicit operator int(CebBase b) => b.Value;

        public override bool Equals(object obj) => (obj is CebBase op && op.Rank == Rank) && Operations.WithIndex().All(e => e.Item1 == op.Operations[e.Item2]);

        public int Compare(CebBase b) => Rank - b.Rank;

        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode() => base.GetHashCode();

        protected CebBase() => Operations = new List<string>();
    }
}