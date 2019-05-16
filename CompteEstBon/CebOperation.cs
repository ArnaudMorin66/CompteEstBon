using System;
using System.Collections.Generic;

namespace CompteEstBon {
    /// <inheritdoc />
    ///    /// /// ///
    /// <summary>
    /// Classe opération
    /// </summary>
    [System.Runtime.InteropServices.Guid("59276C20-8670-47FB-BA13-44A1450CB9BF")]
    public sealed class CebOperation : CebBase {
        public static readonly char[] ListeOperations = { 'x', '+', '-', '/' };
        private List<string> _operations;

        /// <summary> Constructor opération <=> g op d </summary> <param name="g"> </param> <param
        /// name="op"> </param> <param name="d"> </param>
        public CebOperation(CebBase g, char op, CebBase d) {
            Left = g;
            Oper = op;
            Right = d;

            if (g <= 0 || d <= 0) {
                Value = 0;
                return;
            }

            Value = op switch
            {
                '+' => g + d,
                '-' => Math.Max(0, g - d),
                'x' => g <= 1 || d <= 1 ? 0 : g * d,
                '/' => d <= 1 || g % d != 0 ? 0 : g / d,
                _ => 0
            };
        }


        public CebBase Left { get; }
        public CebBase Right { get; }
        public char Oper { get; }

        /// <summary>
        /// Sert de fonction de hachage par défaut.
        /// </summary>
        /// <returns>
        /// Code de hachage pour l'objet actuel.
        /// </returns>
        public override int GetHashCode() {
            unchecked {
                return ((391 + (Left?.GetHashCode() ?? 0)) * 23
                        + (Right?.GetHashCode() ?? 0)) * 23
                       + Oper.GetHashCode();
            }
        }


        /// <summary>
        /// Renvoie le rang
        /// </summary>
        public override int Rank => Operations.Count;


        /// <inheritdoc />
        public override List<string> Operations {
            get {
                if (_operations != null) return _operations;
                _operations = new List<string>();
                if (Left is CebOperation) _operations.AddRange(Left.Operations);
                if (Right is CebOperation) _operations.AddRange(Right.Operations);
                _operations.Add($"{Left.Value} {Oper} {Right.Value} = {Value}");
                return _operations;
            }
        }

        public override bool IsValid => Value > 0;

        private string _string;

        /// <summary>
        /// Convertion en chaine de l(opération
        /// </summary>
        /// <returns>
        /// </returns>
        public override string ToString() {
            return _string ?? (_string = string.Join(", ", Operations));
        }

        /// <summary> Test égalité
        public override bool Equals(object obj) {
            return obj is CebOperation op
? op.Value == Value
&& op.Oper == Oper
&& ((op.Left.Equals(Left) && op.Right.Equals(Right))
|| (op.Left.Equals(Right) && op.Right.Equals(Left)))
: false;
        }

        /// <inheritdoc />
        public override CebDetail ToCebDetail {
            get {
                var elt = new CebDetail();
                var ty = typeof(CebDetail);
                foreach (var (op, i) in Operations.WithIndex()) {
                    ty.GetProperties()[i].SetValue(elt, op);
                }
                return elt;
            }
        }

    }
}