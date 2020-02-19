using System;
using System.Runtime.InteropServices;

namespace CompteEstBon
{
    /// <inheritdoc />
    /// /// /// ///
    /// <summary>
    ///     Classe op�ration
    /// </summary>
    [Guid("59276C20-8670-47FB-BA13-44A1450CB9BF")]
    public sealed class CebOperation : CebBase
    {
        public static readonly char[] AllOperations = { 'x', '+', '-', '/' };
        /// <summary>
        ///     Constructor op�ration (g op d)
        /// </summary>
        /// <param name="g"> </param>
        /// <param
        ///     name="op">
        /// </param>
        /// <param name="d"> </param>
        public CebOperation(CebBase g, char op, CebBase d)
        {
            if(g.Value < d.Value) {
                (g, d) = (d, g);
            }
            Value = op switch
            {
                '+' => g.Value + d.Value,
                '-' => Math.Max(0, g.Value - d.Value),
                'x' => g.Value <= 1 || d.Value <= 1 ? 0 : g.Value * d.Value,
                '/' => d.Value <= 1 || g.Value % d.Value != 0 ? 0 : g.Value / d.Value,
                _ => 0
            };
            if (Value == 0) return;
            Add(g);
            Add(d);
            this[Rank++] = $"{g.Value} {op} {d.Value} = {Value}";
        }

        /// <summary>
        ///     Renvoie le rang
        /// </summary>

        public override bool IsValid => Value > 0;
    }
}