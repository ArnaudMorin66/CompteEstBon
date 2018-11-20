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
    public sealed class CebOperation : CebBase
    {
        public static readonly char[] ListeOperations = { 'x', '+', '-', '/' };

        /// <summary> Constructor opération <=> g op d </summary> <param name="g"> </param> <param
        /// name="op"> </param> <param name="d"> </param>
        public CebOperation(CebBase g, char op, CebBase d) {
            _left = g;
            _oper = op;
            _right = d;
            Evaluate();
        }

        /// <summary>
        /// Opérateur qauche
        /// </summary>
        private CebBase _left;

        /// <summary>
        /// Opérateur droit
        /// </summary>
        private CebBase _right;

        /// <summary>
        /// Opération +,-,/,*
        /// </summary>
        private char _oper;

        /// <summary>
        /// Sert de fonction de hachage par défaut.
        /// </summary>
        /// <returns>
        /// Code de hachage pour l'objet actuel.
        /// </returns>
        public override int GetHashCode() {
            unchecked {
                return ((391
                         + (_left?.GetHashCode() ?? 0)) * 23
                        + (_right?.GetHashCode() ?? 0)) * 23
                       +    _oper.GetHashCode();
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
            var g = _left.Value;
            var oper = _oper;
            var d = _right.Value;

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
            _rank = _left.Rank + _right.Rank;
        }

        private string _string;

        /// <summary>
        /// Convertion en chaine de l(opération
        /// </summary>
        /// <returns>
        /// </returns>
        public override string ToString() {
            return _string ?? (_string = string.Join(", ", Operations));
        }

        private List<string> _array;

        public override List<string> Operations
        {
            get {
                if (_array == null) {
                    _array = new List<string>();
                    if (_left is CebOperation)
                        _array.AddRange(_left.Operations);
                    if (_right is CebOperation)
                        _array.AddRange(_right.Operations);
                    _array.Add($"{_left.Value} {_oper} {_right.Value} = {Value}");
                }
                return _array;
            }
        }

        /// <summary> Test égalité
        public override bool Equals(object obj) {
            if (!(obj is CebOperation))
                return false;
            var op = obj as CebOperation;
            return (op._oper == _oper)
                   && (op.Value == Value)
                   && ((op._left.Equals(_left) && op._right.Equals(_right))
                    || (op._left.Equals(_right) && op._right.Equals(_left)));
        }
    }
}