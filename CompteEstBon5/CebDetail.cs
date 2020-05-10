using System.Collections.Generic;
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
                for (var i = 0; i < 5; i++) {
                    var tmp = this[i];
                    if (tmp == null)
                        break;
                    yield return tmp;
                }
            }
        }

        public string this[int i] {
            get => GetType().GetProperty($"Op{i + 1}").GetValue(this) as string;
            set => GetType().GetProperty($"Op{i + 1}").SetValue(this, value);
        }      
        public (int gauche, char op, int droite) Decoup(int i) {
            var l = this[i].Split();
            if (int.TryParse(l[0], out int g)) return (0, '\0', 0);
            if (this is CebPlaque) return (g, '\0', 0);
            if (!int.TryParse(l[2], out int d)) return (0, '\0', 0);
            return (g, l[1][0], d);
        }
    }
}