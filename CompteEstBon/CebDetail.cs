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
        public CebDetail(CebBase ceb) : this(ceb.Operations) { }
        public CebDetail(IEnumerable<string> op) {
            var type = typeof(CebDetail);
            foreach (var (o, i) in op.WithIndex()) {
                type.GetProperty($"Op{i + 1}")?.SetValue(this, o);
            }
        }
        public override string ToString() => string.Join(", ", GetType().GetProperties()
            .Where(item => item.Name.StartsWith("Op"))
            .OrderBy(item => item.Name)
            .Select(o => o.GetValue(this) as string)
            .Where(v => !string.IsNullOrEmpty(v)));

        public static implicit operator CebDetail(List<string> lt) => new CebDetail(lt);
        public static implicit operator CebDetail(CebBase bs) => new CebDetail(bs);
    }
}