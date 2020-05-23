using System.Collections.Generic;

namespace CompteEstBon {
    public interface ICebDetail {
        public string Op1 { get; set; }
        public string Op2 { get; set; }
        public string Op3 { get; set; }
        public string Op4 { get; set; }
        public string Op5 { get; set; }
    }
    public class CebDetail: ICebDetail {
        public string Op1 { get; set; }
        public string Op2 { get; set; }
        public string Op3 { get; set; }
        public string Op4 { get; set; }
        public string Op5 { get; set; }
        public override string ToString() => string.Join(", ", Operations);

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
    }
}