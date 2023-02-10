using System.Xml.Serialization;

namespace CompteEstBon;

public record struct CebData {
    /// <summary>
    /// 
    /// </summary>
    public int Search { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [XmlArray] [XmlArrayItem("Plaque")] public int[] Plaques { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? Status { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Found { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? Diff { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [XmlArray] [XmlArrayItem("Solution")] public IEnumerable<string>? Solutions { get; set; }
}