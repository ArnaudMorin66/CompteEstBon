//-----------------------------------------------------------------------
// <copyright file="CebData.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#nullable enable
using System.Xml.Serialization;

namespace CompteEstBon;

/// <summary>
/// Données de ceb
/// </summary>
public record struct CebData {
    /// <summary>
    ///
    /// </summary>
    public  int Search { get; set; }

    /// <summary>
    ///
    /// </summary>
    [XmlArray, XmlArrayItem("Plaque")]
    public  int[] Plaques { get; set; }

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
    public  int? Ecart { get; set; }

    /// <summary>
    ///
    /// </summary>
    [XmlArray,XmlArrayItem("Solution")]
    public string[]? Solutions { get; set; }
}