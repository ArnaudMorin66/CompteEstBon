﻿using System.Linq;

namespace CompteEstBon {
    public class CebDetail {
        public string Op1 { get; set; }
        public string Op2 { get; set; }
        public string Op3 { get; set; }
        public string Op4 { get; set; }
        public string Op5 { get; set; }
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