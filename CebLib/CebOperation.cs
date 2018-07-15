using System;
using System.Collections.Generic;

namespace CompteEstBon {

    /// <inheritdoc />
    /// <summary>
    /// Classe op�ration 
    /// </summary>
    [System.Runtime.InteropServices.Guid("59276C20-8670-47FB-BA13-44A1450CB9BF")]
    public class CebOperation : CebBase {
        public static readonly char[] ListeOperations = { 'x', '+', '-', '/' };

        /// <summary>
        /// Constructor op�ration <=> g op d
        /// </summary>
        /// <param name="g">
        /// </param>
        /// <param name="op">
        /// </param>
        /// <param name="d">
        /// </param>
        public CebOperation(CebBase g, char op, CebBase d) {
            Left = g;
            Oper = op;
            Right = d;
            Value = Evaluate();
        }

        /// <summary>
        /// Op�rateur qauche
        /// </summary>
        public CebBase Left { get; }

        /// <summary>
        /// Op�rateur droit
        /// </summary>
        public CebBase Right { get; }

        /// <summary>
        /// Op�ration +,-,/,*
        /// </summary>
        public char Oper { get; }

        /// <summary>Sert de fonction de hachage par d�faut. </summary>
        /// <returns>Code de hachage pour l'objet actuel.</returns>
        public override int GetHashCode() {
            unchecked {
                return ((391
                         + (Left?.GetHashCode() ?? 0)) * 23
                        + (Right?.GetHashCode() ?? 0)) * 23
                       + Oper.GetHashCode();
            }
        }

        private int _rank = -1;

        /// <summary>
        /// Renvoie le rang
        /// </summary>
        public override int Rank {
            get {
                if (_rank == -1) {
                    _rank = 1;
                    if (Left is CebOperation)
                        _rank += Left.Rank;
                    if (Right is CebOperation)
                        _rank += Right.Rank;
                }
                return _rank;
            }
        }

        /// <summary>
        /// Evalue l'op�ration
        /// </summary>
        /// <returns>
        /// </returns>
        /// <exception cref="ArithmeticException"></exception>
        private int Evaluate() {
            var g = Left.Value;
            var oper = Oper;
            var d = Right.Value;

            if (g <= 0 || d <= 0) {
                return 0;
            }

            switch (oper) {
                case '+':
                    return g + d;

                case '-':
                    return Math.Max(0, g - d);

                case 'x':
                    return (g <= 1 || d <= 1) ? 0 : g * d;

                case '/':
                    return (d <= 1 || g % d != 0) ? 0 : g / d;

                default:
                    throw new ArithmeticException();
            }
        }

        private string _string;

        /// <summary>
        /// Convertion en chaine de l(op�ration
        /// </summary>
        /// <returns>
        /// </returns>
        public override string ToString() {
            if (_string == null) {
                _string = this[0];
                for (int i = 1; i < _array.Count; i++) {
                    _string += $", {this[i]}";
                }
            }
            return _string;
        }

        private List<string> _array;

        public override List<string> Operations {
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

        /// <summary>
        /// Test �galit�
        public override bool Equals(object obj) {
            if (!(obj is CebOperation))
                return false;
            var op = (CebOperation)obj;

            return (op.Oper == Oper)
                   && (op.Value == Value)
                   && ((op.Left == Left && op.Right == Right));
        }
    }
}