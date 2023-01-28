using System.Xml.Serialization;

namespace CompteEstBon {
    public record struct CebData {
        public int Search { get; set; }
        [XmlArray()]
        [XmlArrayItem("Plaque")]
        public int[] Plaques { get; set; }
        public CebStatus? Status { get; set; }
        public string Found { get; set; }
        public int? Diff { get; set; }
        [XmlArray]
        [XmlArrayItem("Solution")]
        public string[] Solutions { get; set; }
    }
}
