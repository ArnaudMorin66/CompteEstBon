//-----------------------------------------------------------------------
// <copyright file="CebDetail.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Text.Json.Serialization;

namespace CompteEstBon {
    public class CebDetail {
        public string Op1 { get; set; }

        public string Op2 { get; set; }

        public string Op3 { get; set; }

        public string Op4 { get; set; }

        public string Op5 { get; set; }

        public override string ToString() => string.Join(", ", Operations);

        [JsonIgnore]
        public IEnumerable<string> Operations {
            get {
                for (var i = 0; i < 5 && Op(i) != null; i++)
                    yield return Op(i);
            }
        }

        public (int gauche, char op, int droite) Split(int i) {
            var l = Op(i).Split();
            return int.TryParse(l[0], out var g) ? (0, '\0', 0) :
                !int.TryParse(l[2], out var d) ? (0, '\0', 0) : (g, l[1][0], d);
        }

        public string Op(int i) => GetType().GetProperty($"Op{i + 1}")!.GetValue(this) as string;

        public void SetOp(int i, string value) => GetType().GetProperty($"Op{i + 1}")!.SetValue(this, value);
    }
}