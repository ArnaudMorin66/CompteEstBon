using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
// using System.Text.Json.Serialization;

namespace CompteEstBon {

    public class CebDetail {
        public string Op1 { get; set; }
        public string Op2 { get; set; }
        public string Op3 { get; set; }
        public string Op4 { get; set; }
        public string Op5 { get; set; }

        public override string ToString() => string.Join(", ", Operations);

        // [JsonIgnore]
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

            set {
                GetType().GetProperty($"Op{i + 1}").SetValue(this, value);
            }
        }
    }
}