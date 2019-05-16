using System.Linq;

namespace CompteEstBon {
    public class CebDetail {
        public string op1 { get; set; }
        public string op2 { get; set; }
        public string op3 { get; set; }
        public string op4 { get; set; }
        public string op5 { get; set; }
        public override string ToString() {
            var result = string.Empty;
            foreach (var item in GetType().GetProperties()
                .Where(item => item.Name.StartsWith("op"))) {
                var value = item.GetValue(this) as string;
                if (string.IsNullOrEmpty(value)) break;
                result += (string.IsNullOrEmpty(result) ? "" : ", ") + $"{item.Name} = { value }";
            }
            return result;
        }
    }
}