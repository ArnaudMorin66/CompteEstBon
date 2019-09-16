using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace CompteEstBon {
    /// <inheritdoc />
    /// /// /// ///
    /// <summary>
    ///     Classe opération
    /// </summary>
    [Guid("59276C20-8670-47FB-BA13-44A1450CB9BF")]
    public sealed class CebOperation : CebBase {
        public static readonly char[] ListeOperations = {'x', '+', '-', '/'};
        // private List<string> _operations;

        /// <summary>
        ///     Constructor opération <=> g op d
        /// </summary>
        /// <param name="g"> </param>
        /// <param
        ///     name="op">
        /// </param>
        /// <param name="d"> </param>
        public CebOperation(CebBase g, char op, CebBase d) {
            if (g <= 0 || d <= 0) {
                Value = 0;
                return;
            }

            Value = op switch {
                '+' => g.Value + d.Value,
                '-' => Math.Max(0, g.Value - d.Value),
                'x' => g.Value <= 1 || d.Value <= 1 ? 0 : g.Value * d.Value,
                '/' => d.Value <= 1 || g.Value % d.Value != 0 ? 0 : g.Value / d.Value,
                _ => 0
            };

            if (g is CebOperation) Operations.AddRange(g.Operations);
            if (d is CebOperation) Operations.AddRange(d.Operations);
            Operations.Add($"{g.Value} {op} {d.Value} = {Value}");
            Rank = Operations.Count;
        }


        /// <summary>
        ///     Renvoie le rang
        /// </summary>

        public override bool IsValid => Value > 0;

        //public override CebDetail Detail {
        //    get {
        //        return new CebDetail(this);
        //    }
        //}


        public override int GetHashCode() {
            unchecked {
                return Operations.Sum(o => (391 + o.GetHashCode()) * 23);
            }
        }

        /// <summary>
        ///     Convertion en chaine de l(opération
        /// </summary>
        /// <returns>
        /// </returns>
        public override string ToString() => string.Join(", ", Operations);

        /// <summary> Test égalité
        public override bool Equals(object obj) => !(obj is CebOperation op) || op.Value != Value || op.Operations.Count() != Operations.Count()
                ? false
                : Operations.WithIndex().All(elt => op.Operations[elt.Item2] == elt.Item1);
    }
}