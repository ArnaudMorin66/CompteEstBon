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
///     Données de ceb
/// </summary>
public record struct CebData {
	/// <summary>
	///     Valeur recherché
	/// </summary>
	public int Search { get; set; }

	/// <summary>
	///     Liste desplaques
	/// </summary>
	[XmlArray, XmlArrayItem("Plaque")]
	public int[] Plaques { get; set; }

	/// <summary>
	///     Status
	/// </summary>
	public string? Status { get; set; }

	/// <summary>
	///     Found
	/// </summary>
	public string Found { get; set; }

	/// <summary>
	///     Ecart
	/// </summary>
	public int? Ecart { get; set; }

	/// <summary>
	///     Solutions
	/// </summary>
	[XmlArray, XmlArrayItem("Solution")]
	public List<string> Solutions { get; set; }
}