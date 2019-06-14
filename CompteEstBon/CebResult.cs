using System.Collections.Generic;
using CompteEstBon;

namespace CompteEstBon {
    public struct CebResult {
        public int Search { get; set; }
        public IEnumerable<int> Plaques { get; set; }
        public CebStatus Status { get; set; }
        public string Found { get; set; }
        public int Diff { get; set; }
        public IEnumerable<IEnumerable<string>> Solutions { get; set; }
  }
}
