using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CompteEstBon.ViewModel {
    public class CebDetail {
        public string Op1 {get;private set; }
        public string Op2 {get;private set; }
        public string Op3 {get;private set; }
        public string Op4 {get;private set; }
        public string Op5 {get;private set; }
        public static CebDetail FromCebBase(CebBase sol) {
            CebDetail detail = new();
            int ix = 1 ;
            foreach (var el in sol.Operations) {
                detail.GetType().GetProperty($"Op{ix++}")?.SetValue(detail, el);
            }

            return detail;

        }
    }
}
