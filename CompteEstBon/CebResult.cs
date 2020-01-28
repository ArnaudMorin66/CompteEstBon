using System;
using System.Collections.Generic;

namespace CompteEstBon {
    public struct CebResult {
        public int Search { get; set; }
        public IEnumerable<int> Plaques { get; set; }
        public CebStatus? Status { get; set; }
  
        public  String Found { get; set; }
        public int? Diff { get; set; }
        public IEnumerable<CebDetail> Solutions { get; set; }
    }
}
