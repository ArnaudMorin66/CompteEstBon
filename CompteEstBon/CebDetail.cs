using System.Linq;

namespace CompteEstBon {
    public class CebDetail {
        public string Op1 { get; set; }
        public string Op2 { get; set; }
        public string Op3 { get; set; }
        public string Op4 { get; set; }
        public string Op5 { get; set; }
        public override string ToString() => string.Join(", ", GetType().GetProperties()
                .Where(item => item.Name.StartsWith("Op"))
                    .Select(o => o.GetValue(this) as string).Where(v => !string.IsNullOrEmpty(v)));//foreach (var item in GetType().GetProperties()//    .Where(item => item.Name.StartsWith("Op"))) {//    var value = item.GetValue(this) as string;//    if (string.IsNullOrEmpty(value)) break;//    result += $"{(string.IsNullOrEmpty(result) ? "" : ", ")}{ value }";//}//return result;
    }
}