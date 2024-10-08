﻿//-----------------------------------------------------------------------
// <copyright file="CebSerialize.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

using Syncfusion.DocIO;

namespace CompteEstBon;

public static class CebSerialize {
	public static readonly Dictionary<string, Action<CebTirage, FileInfo>> ListeFormats =
		new() {
			[".zip"] = SaveZip, [".json"] = SaveJson, [".xml"] = SaveXml, [".xlsx"] = SaveXlsx, [".docx"] = SaveDocx,
			[".html"] = SaveDocx
		};

	/// <summary>
	/// </summary>
	public static readonly JsonSerializerOptions JsonOptions = new() {
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, Converters = { new JsonStringEnumConverter() },
		WriteIndented = true
	};

	public static bool Export(this CebTirage tirage, FileInfo fi) {
		if (!ListeFormats.TryGetValue(fi.Extension, out var laction)) return false;
		if (fi.Exists)
			fi.Delete();
		laction(tirage, fi);
		return true;
	}

	public static bool Export(this CebTirage tirage, string path) => Export(tirage, new FileInfo(path));

	/// <summary>
	/// </summary>
	/// <param name="tirage"></param>
	/// <param name="file"></param>
	public static void SaveJson(this CebTirage tirage, FileInfo file) {
		using var stream = file.Create();
		tirage.JsonSaveStream(stream);
	}

	/// <summary>
	/// </summary>
	/// <param name="tirage"></param>
	/// <param name="file"></param>
	public static void SaveXlsx(this CebTirage tirage, FileInfo file) {
		using var stream = file.Create();
		tirage.ExcelSaveStream(stream);
	}

	/// <summary>
	/// </summary>
	/// <param name="tirage"></param>
	/// <param name="file"></param>
	public static void SaveDocx(this CebTirage tirage, FileInfo file) {
		using var stream = file.Create();
		tirage.WordSaveStream(stream, file.Extension == ".html" ? FormatType.Html : FormatType.Docx);
	}

	/// <summary>
	/// </summary>
	/// <param name="tirage"></param>
	/// <param name="file"></param>
	public static void SaveXml(this CebTirage tirage, FileInfo file) {
		using var stream = file.Create();
		tirage.XmlSaveStream(stream);
	}

	/// <summary>
	/// </summary>
	/// <param name="tirage"></param>
	public static string WriteJson(this CebTirage tirage) => JsonSerializer.Serialize(tirage.Resultat, JsonOptions);

	/// <summary>
	/// </summary>
	/// <param name="tirage"></param>
	/// <param name="file"></param>
	public static void SaveZip(this CebTirage tirage, FileInfo file) {
		using var archive = ZipFile.Open(file.FullName, ZipArchiveMode.Update, Encoding.UTF8);
		var num = new[] { 0 }.Concat(
				archive.Entries.Select(
					p => int.TryParse(p.Name[..p.Name.LastIndexOf('.')], out var result) ? result : 0))
			.Max();
		ZipStream(archive, $"{++num:000000}.json", tirage.JsonSaveStream);
		ZipStream(archive, $"{++num:000000}.xml", tirage.XmlSaveStream);
	}

	/// <summary>
	/// </summary>
	/// <param name="archive"></param>
	/// <param name="nom"></param>
	/// <param name="action"></param>
	public static void ZipStream(ZipArchive archive, string nom, Action<Stream> action) {
		var stream = archive.CreateEntry(nom, CompressionLevel.SmallestSize).Open();
		action(stream);
		stream.Close();
	}

	/// <summary>
	/// </summary>
	/// <param name="tirage"></param>
	/// <param name="stream"></param>
	public static void JsonSaveStream(this CebTirage tirage, Stream stream) => JsonSerializer.Serialize(
		stream,
		tirage.Resultat,
		JsonOptions);

	/// <summary>
	/// </summary>
	/// <param name="tirage"></param>
	/// <param name="stream"></param>
	/// <exception cref="Exception"></exception>
	public static void XmlSaveStream(this CebTirage tirage, Stream stream) {
		XmlSerializer mySerializer = new(typeof(CebData));
		try {
			mySerializer.Serialize(stream, tirage.Resultat);
		} catch (SerializationException) {
			throw new Exception("Erreur serialisation");
		}
	}

	/// <summary>
	/// </summary>
	/// <param name="tirage"></param>
	/// <param name="fichiers"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public static void SerializeFichiers(this CebTirage tirage, IEnumerable<FileInfo> fichiers) {
		foreach (var fichier in fichiers) tirage.Export(fichier);
	}
}