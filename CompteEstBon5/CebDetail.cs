using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CompteEstBon {
    public interface ICebDetail {
        public string Op1 { get; set; } 
        public string Op2 { get; set; }
        public string Op3 { get; set; }
        public string Op4 { get; set; }
        public string Op5 { get; set; }

    }
    public class TDetail {
        private CebDetail _detail;
        public string Op1 { get => _detail?.Op1; set { Detail.Op1 = value; } }
        public string Op2 { get => _detail?.Op2; set { Detail.Op2 = value; } }
        public string Op3 { get => _detail?.Op3; set { Detail.Op3 = value; } }
        public string Op4 { get => _detail?.Op4; set { Detail.Op4 = value; } }
        public string Op5 { get => _detail?.Op5; set { Detail.Op5 = value; } }
        public TDetail(CebDetail d) {
            _detail = d;
        }
        public CebDetail Detail => _detail ?? new CebDetail();

    }

    public class CebDetail: ICebDetail {
        public string Op1 { get; set; }
        public string Op2 { get; set; }
        public string Op3 { get; set; }
        public string Op4 { get; set; }
        public string Op5 { get; set; }
        public override string ToString() => string.Join(", ", Operations);

        [JsonIgnore]
        public IEnumerable<string> Operations {
            get {
                for (var i = 0; i < 5 && this[i] != null; i++) {
                    yield return this[i];
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