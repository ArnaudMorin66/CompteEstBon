using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace CompteEstBon {

    public class CebDetail {
        public string Op1 { get; set; }
        public string Op2 { get; set; }
        public string Op3 { get; set; }
        public string Op4 { get; set; }
        public string Op5 { get; set; }

        public CebDetail() { }
        public CebDetail(IEnumerable<string> op) {
            for (var i = 0; i < op.Count(); i++) {
                this[i] = op.ElementAt(i);
            }
        }
        public override string ToString() => string.Join(", ", GetType().GetProperties()
            .Where(item => item.Name.StartsWith("Op"))
            .OrderBy(item => item.Name)
            .Select(o => o.GetValue(this) as string)
            .Where(v => !string.IsNullOrEmpty(v)));

        public static implicit operator CebDetail(CebBase bs) => new CebDetail(bs.Operations);

        public string this[int i] {
            get {
                if (i > 4) throw new ArgumentException();
                return GetType().GetProperty($"Op{i + 1}").GetValue(this) as string;
            }
            set {
                if (i > 4) throw new ArgumentException();
                GetType().GetProperty($"Op{i + 1}").SetValue(this, value);
            }
        }
    }
}