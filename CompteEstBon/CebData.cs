using System;
using System.Collections.Generic;

namespace CompteEstBon {
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public struct CebData {
#pragma warning restore CA1815 // Override equals and operator equals on value types
        public int Search { get; set; }
        public IEnumerable<int> Plaques { get; set; }
        public CebStatus? Status { get; set; }
  
        public string Found { get; set; }
        public int? Diff { get; set; }
        public IEnumerable<CebDetail> Solutions { get; set; }
    }
}
