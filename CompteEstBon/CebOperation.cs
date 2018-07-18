using System;
using System.Collections.Generic;

namespace CompteEstBon
{
    /// <inheritdoc />
    ///    /// /// ///
    /// <summary>
    /// Classe opération
    /// </summary>
    [System.Runtime.InteropServices.Guid("59276C20-8670-47FB-BA13-44A1450CB9BF")]
    public class CebOperation : CebBase
    {
        public static readonly char[] ListeOperations = { 'x', '+', '-', '/' };

        /// <summary> Constructor opération <=> g op d </summary> <param name="g"> </param> <param
        /// name="op"> </param> <param name="d"> </param>
        public CebOperation(CebBase g, char op, CebBase d) {
            Left = g;
            Oper = op;
            Right = d;
            Evaluate();
        }

        /// <summary>
        /// Opérateur qauche
        /// </summary>
        public CebBase Left { get; }

        /// <summary>
        /// Opérateur droit
        /// </summary>
        public CebBase Right { get; }

        /// <summary>
        /// Opération +,-,/,*
        /// </summary>
        public char Oper { get; }

        /// <summary>
        /// Sert de fonction de hachage par défaut.
        /// </summary>
        /// <returns>
        /// Code de hachage pour l'objet actuel.
        /// </returns>
        public override int GetHashCode() {
            unchecked {
                return ((391
                         + (Left?.GetHashCode() ?? 0)) * 23
                        + (Right?.GetHashCode() ?? 0)) * 23
                       + Oper.GetHashCode();
            }
        }

        private int _rank = 0;

        /// <summary>
        /// Renvoie le rang
        /// </summary>
        public override int Rank => _rank;

        /// <summary>
        /// Evalue l'opération
        /// </summary>
        /// <returns>
        /// </returns>
        /// <exception cref="ArithmeticException">
        /// </exception>
        private void Evaluate() {
            var g = Left.Value;
            var oper = Oper;
            var d = Right.Value;

            if (g <= 0 || d <= 0) {
                Value = 0;
            } else {
                switch (oper) {
                    case '+':
                        Value = g + d;
                        break;

                    case '-':
                        Value = Math.Max(0, g - d);
                        break;

                    case 'x':
                        Value = (g <= 1 || d <= 1) ? 0 : g * d;
                        break;

                    case '/':
                        Value = (d <= 1 || g % d != 0) ? 0 : g / d;
                        break;

                    default:
                        throw new ArithmeticException();
                }
            }
            _rank = Left.Rank + Right.Rank;
        }

        private string _string;

        /// <summary>
        /// Convertion en chaine de l(opération
        /// </summary>
        /// <returns>
        /// </returns>
        public override string ToString() {
            return _string ?? (_string = string.Join(",", Operations));
        }

        private List<string> _array;

        public override List<string> Operations
        {
            get {
                if (_array == null) {
                    _array = new List<string>();
                    if (Left is CebOperation)
                        _array.AddRange(Left.Operations);
                    if (Right is CebOperation)
                        _array.AddRange(Right.Operations);
                    _array.Add($"{Left.Value} {Oper} {Right.Value} = {Value}");
                }
                return _array;
            }
        }

        /// <summary> Test égalité
        public override bool Equals(object obj) {
            if (!(obj is CebOperation))
                return false;
            var op = obj as CebOperation;
            return (op.Oper == Oper)
                   && (op.Value == Value)
                   && ((op.Left.Equals(Left) && op.Right.Equals(Right))
                    || (op.Left.Equals(Right) && op.Right.Equals(Left)));
        }
    }
}