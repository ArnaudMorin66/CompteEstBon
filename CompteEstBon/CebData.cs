//-----------------------------------------------------------------------
// <copyright file="CebData.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Xml.Serialization;

namespace CompteEstBon;

public record struct CebData {
    /// <summary>
    ///
    /// </summary>
    public required int Search { get; set; }

    /// <summary>
    ///
    /// </summary>
    [XmlArray, XmlArrayItem("Plaque")]
    public required int[] Plaques { get; set; }

    /// <summary>
    ///
    /// </summary>
    public required string? Status { get; set; }

    /// <summary>
    ///
    /// </summary>
    public required string Found { get; set; }

    /// <summary>
    ///
    /// </summary>
    public required int? Diff { get; set; }

    /// <summary>
    ///
    /// </summary>
    [XmlArray,XmlArrayItem("Solution")]
    public required string[]? Solutions { get; set; }
}