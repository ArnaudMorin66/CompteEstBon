using System;
using System.Collections.Generic;

namespace CompteEstBon
{
    /// <inheritdoc />
    ///  <summary>
    ///  </summary>
    [System.Runtime.InteropServices.Guid("59276C20-8670-47FB-BA13-44A1450CB9BF")]
    public class CebOperation : CebBase
  {
    public static readonly char[] ListeOperations = {'x', '+', '-', '/'};
    /// <summary>
    ///
    /// </summary>
    /// <param name="g"></param>
    /// <param name="op"></param>
    /// <param name="d"></param>
    public CebOperation(CebBase g, char op, CebBase d)
    {
      Left = g;
      Oper = op;
      Right = d;
      Value = Evaluate();
    }

    /// <summary>
    ///
    /// </summary>
    public CebBase Left { get; }

    /// <summary>
    ///
    /// </summary>
    public CebBase Right { get; }

    /// <summary>
    ///
    /// </summary>
    public char Oper { get; }

    public override int GetHashCode()
    {
      unchecked
      {
        return ((391
                 + (Left?.GetHashCode() ?? 0)) * 23
                + (Right?.GetHashCode() ?? 0)) * 23
               + Oper.GetHashCode();
      }
    }

    private int _rank = -1;

    /// <summary>
    ///
    /// </summary>
    public override int Rank
    {
      get
      {
        if (_rank == -1)
        {
          _rank = 1;
          if (Left is CebOperation)
            _rank += Left.Rank;
          if (Right is CebOperation)
             _rank +=  Right.Rank;
        }
        return _rank;
      }
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    private int Evaluate()
    {
      var g = Left.Value;
      var oper = Oper;
      var d = Right.Value;

      if (g <= 0 || d <= 0)
      {
        return 0;
      }

      switch (oper)
      {
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
    ///
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (_string == null)
      {
        _string = this[0];
        for (int i = 1; i < _array.Length; i++)
        {
          _string += $", {this[i]}";
        }
      }
      return _string;
    }

    private string[] _array;

    public override string[] Operations
    {
      get
      {
        if (_array == null)
        {
          var l = new List<string>();
          if (Left is CebOperation)
            l.AddRange(Left.Operations);
          if (Right is CebOperation)
            l.AddRange(Right.Operations);
          l.Add($"{Left.Value} {Oper} {Right.Value} = {Value}");
          _array = l.ToArray();
        }
        return _array;
      }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      if (!(obj is CebOperation))
        return false;
      var op = (CebOperation) obj;

      return (op.Oper == Oper)
             && (op.Value == Value)
             && ((op.Left == Left && op.Right == Right));
    }
  }
}