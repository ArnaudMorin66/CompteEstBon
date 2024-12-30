using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace arnaud.morin.outils;

/// <summary>
///     Fournit des méthodes d'extension pour les chaines de caractères
/// </summary>
// ReSharper disable once PartialTypeWithSinglePart
public static partial class StringExtensions {
	// ReSharper disable InconsistentNaming
	private static MD5? _md5;

	private static SHA1? _sha1;

	// ReSharper disable InconsistentNaming
	// private static readonly Regex _emailRegex =
	//   MyRegex();

	/// <summary>
	///     Indique si une chaine est nulle ou vide
	/// </summary>
	/// <param name="s">la chaine à tester</param>
	/// <returns>true si la chaine est nulle ou vide, false sinon</returns>
	public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

	/// <summary>
	/// </summary>
	/// <param name="value"></param>
	/// <param name="paramName"></param>
	/// <exception cref="ArgumentException"></exception>
	public static void CheckNullOrEmpty(this string value, string paramName) {
		if (value.IsNullOrEmpty())
			throw new ArgumentException(paramName);
	}

	/// <summary>
	///     Indique si une chaîne est nulle, vide ou composée uniquement d'espaces blancs.
	/// </summary>
	/// <param name="s">la chaine à tester</param>
	/// <returns>true si la chaine est nulle, vide ou composée uniquement d'espaces blancs, false sinon</returns>
	public static bool IsNullOrWhiteSpace(this string s) => string.IsNullOrEmpty(s) || string.IsNullOrEmpty(s.Trim());

	/// <summary>
	///     Formate la chaine avec les valeurs spécifiées, de la même façon que
	///     String.Format
	/// </summary>
	/// <param name="format">La chaine de format</param>
	/// <param name="args">les valeurs à utiliser</param>
	/// <returns>La chaine formatée</returns>
	public static string FormatWith(this string format, params object[] args) => string.Format(format, args);

	/// <summary>
	///     Inverse l'ordre des caractères d'une chaine
	/// </summary>
	/// <param name="s">La chaine à inverser</param>
	/// <returns>La chaine inversée</returns>
	public static string Reverse(this string s) {
		var wasFormD = false;
		if (s.IsNormalized(NormalizationForm.FormD)) {
			wasFormD = true;
			s = s.Normalize(NormalizationForm.FormC);
		}

		s = new string((s as IEnumerable<char>).Reverse().ToArray());

		if (wasFormD) s = s.Normalize(NormalizationForm.FormD);
		return s;
	}

	/// <summary>
	///     Concatène toutes les chaines de la liste en plaçant le séparateur
	///     spécifié entre chaque chaine
	/// </summary>
	/// <param name="list">La liste de chaines à concaténer</param>
	/// <param name="separator">Le séparateur à utiliser</param>
	/// <returns>La concaténation des chaines de la liste</returns>
	public static string Join(this IEnumerable<string> list, string separator) =>
		string.Join(separator, list.ToArray());

	/// <summary>
	///     Renvoie une chaine construite à partir d'une séquence de caractères
	/// </summary>
	/// <param name="chars">La séquence de caractères à transformer en chaine</param>
	/// <returns>Une chaine constituée des caractères de la séquence</returns>
	public static string Join(this IEnumerable<char> chars) => new(chars.ToArray());

	/// <summary>
	///     Enumère les lignes d'une chaine de caractères
	/// </summary>
	/// <param name="s">La chaine à découper en lignes</param>
	/// <returns>La liste des lignes de cette chaine</returns>
	public static IEnumerable<string> ReadLines(this string s) => ReadLinesIterator(s);

	private static IEnumerable<string> ReadLinesIterator(string s) {
		using var reader = new StringReader(s);
		while (reader.ReadLine() is { } line) yield return line;
	}

	/// <summary>
	///     Renvoie le hash MD5 de la chaîne sous forme d'une chaine hexadécimale, en
	///     se basant sur l'encodage UTF8
	/// </summary>
	/// <param name="s">la chaine dont on veut obtenir le hash MD5</param>
	/// <returns>le hash MD5 sous forme d'une chaine hexadécimale</returns>
	// ReSharper disable once InconsistentNaming
	public static string GetMD5Digest(this string s) => s.GetMD5Digest(Encoding.UTF8);

	/// <summary>
	///     Renvoie le hash MD5 de la chaîne sous forme d'une chaine hexadécimale, en
	///     se basant sur l'encodage spécifié
	/// </summary>
	/// <param name="s">la chaine dont on veut obtenir le hash MD5</param>
	/// <param name="encoding">L'encodage à utiliser</param>
	/// <returns>le hash MD5 sous forme d'une chaine hexadécimale</returns>
	// ReSharper disable once InconsistentNaming
	public static string GetMD5Digest(this string s, Encoding encoding) {
		_md5 ??= MD5.Create();

		var stringBytes = encoding.GetBytes(s);
		var hashBytes = _md5.ComputeHash(stringBytes);
		return hashBytes.ToHexString();
	}

	/// <summary>
	///     Renvoie le hash SHA1 de la chaîne sous forme d'une chaîne hexadécimale, en se basant sur l'encodage UTF8
	/// </summary>
	/// <param name="s">la chaine dont on veut obtenir le hash SHA1</param>
	/// <returns>le hash SHA1 sous forme d'une chaine hexadécimale</returns>
	// ReSharper disable once InconsistentNaming
	public static string GetSHA1Digest(this string s) => s.GetSHA1Digest(Encoding.UTF8);

	/// <summary>
	///     Renvoie le hash SHA1 de la chaîne sous forme d'une chaîne hexadécimale, en se basant sur l'encodage spécifié
	/// </summary>
	/// <param name="s">la chaine dont on veut obtenir le hash SHA1</param>
	/// <param name="encoding">L'encodage à utiliser</param>
	/// <returns>le hash SHA1 sous forme d'une chaine hexadécimale</returns>
	// ReSharper disable once InconsistentNaming
	public static string GetSHA1Digest(this string s, Encoding encoding) {
		_sha1 ??= SHA1.Create();

		var stringBytes = encoding.GetBytes(s);
		var hashBytes = _sha1.ComputeHash(stringBytes);
		return hashBytes.ToHexString();
	}

	/// <summary>
	///     Enlève les caractères diacritiques (accents, cédilles...) d'une chaine en les remplaçant par le
	///     caractère de base.
	/// </summary>
	/// <param name="s">La chaine dont on veut enlever les diacritiques</param>
	/// <returns>La chaine sans les diacritiques</returns>
	public static string RemoveDiacritics(this string s) {
		var formD = s.Normalize(NormalizationForm.FormD);
		var chars = new char[formD.Length];
		var count = 0;
		foreach (var c in formD.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
			chars[count++] = c;
		var noDiacriticsFormD = new string(chars, 0, count);
		return noDiacriticsFormD.Normalize(NormalizationForm.FormC);
	}

	/// <summary>
	///     Retourne une chaîne contenant un nombre spécifié de caractères en partant de la gauche d'une chaîne.
	/// </summary>
	/// <param name="s">chaine dont les caractères situés le plus à gauche sont retournés</param>
	/// <param name="count">Nombre de caractères à retourner</param>
	/// <returns>une chaîne contenant le nombre spécifié de caractères en partant de la gauche de s</returns>
	public static string Left(this string s, int count) => s.Length >= count ? s : s[..count];

	public static string Fill(this string s, int count) => ($"{s}{new string(' ', count)}")[..count];

	/// <summary>
	///     Retourne une chaîne contenant un nombre spécifié de caractères en partant de la droite d'une chaîne.
	/// </summary>
	/// <param name="s">chaine dont les caractères situés le plus à droite sont retournés</param>
	/// <param name="count">Nombre de caractères à retourner</param>
	/// <returns>une chaîne contenant le nombre spécifié de caractères en partant de la droite de s</returns>
	public static string Right(this string s, int count) => s.Length >= count ? s : s[^count..];

	public static string FillRight(this string s, int count) => (new string(' ', count) + s)[^count..];

	/// <summary>
	///     Retourne une chaine tronquée au nombre de caractères spécifié.
	/// </summary>
	/// <param name="s">chaine à tronquer</param>
	/// <param name="count">Nombre maximal de caractères à retourner</param>
	/// <returns>une chaîne tronquée au nombre de caractères spécifié</returns>
	public static string Truncate(this string s, int count) => count > s.Length ? s : s[..count];

	/// <summary>
	///     Convertit la chaine spécifiée en initiales majuscules, selon les paramètres de la culture courante.
	/// </summary>
	/// <param name="s">La chaine à convertir en initiales majuscules</param>
	/// <returns>La chaine spécifiée convertie en initiales majuscules</returns>
	public static string ToTitleCase(this string s) => s.ToTitleCase(null!);

	/// <summary>
	///     Convertit la chaine spécifiée en initiales majuscules, selon les paramètres de la culture spécifiée.
	/// </summary>
	/// <param name="s">La chaine à convertir en initiales majuscules</param>
	/// <param name="culture">La culture à utiliser</param>
	/// <returns>La chaine spécifiée convertie en initiales majuscules</returns>
	public static string ToTitleCase(this string s, CultureInfo? culture) {
		culture ??= CultureInfo.CurrentCulture;
		return culture.TextInfo.ToTitleCase(s);
	}

	/// <summary>
	///     Met en majuscule le premier caractère de la chaine spécifiée, selon les paramètres de la culture courante.
	/// </summary>
	/// <param name="s">La chaine dont le premier caractère est mis en majuscule</param>
	/// <returns>La chaine spécifiée avec le premier caractère en majuscule</returns>
	public static string Capitalize(this string s) => s.Capitalize(null!);

	/// <summary>
	///     Met en majuscule le premier caractère de la chaine spécifiée, selon les paramètres de la culture spécifiée.
	/// </summary>
	/// <param name="s">La chaine dont le premier caractère est mis en majuscule</param>
	/// <param name="culture">La culture à utiliser</param>
	/// <returns>La chaine spécifiée avec le premier caractère en majuscule</returns>
	public static string Capitalize(this string s, CultureInfo? culture) {
		culture ??= CultureInfo.CurrentCulture;
		if (s.Length == 0)
			return s;
		var builder = new StringBuilder(s);
		builder[0] = char.ToUpper(builder[0], culture);
		return builder.ToString();
	}


	/// <summary>
	///     Renvoie le caractère à la position spécifiée, ou le caractère nul (0)
	///     si la position spécifiée est la fin de la chaine.
	/// </summary>
	/// <param name="s">Chaine dont un caractère doit être renvoyé</param>
	/// <param name="index">Position du caractère à renvoyer</param>
	/// <returns>
	///     le caractère à la position spécifiée, ou le caractère nul (0)
	///     si la position spécifiée est la fin de la chaine.
	/// </returns>
	public static char CharAt(this string s, int index) => index < s.Length ? s[index] : '\0';

	/// <summary>
	///     Vérifie si une chaine correspond à un motif avec des caractères "joker" ('*' et '?')
	/// </summary>
	/// <param name="text">Chaine à vérifier</param>
	/// <param name="pattern">Motif avec lequel comparer la chaine</param>
	/// <returns>true si la chaine correspond au motif, false sinon.</returns>
	public static bool MatchesWildcard(this string text, string pattern) {
		var it = 0;
		while (text.CharAt(it) != 0 &&
		       pattern.CharAt(it) != '*') {
			if (pattern.CharAt(it) != text.CharAt(it) && pattern.CharAt(it) != '?')
				return false;
			it++;
		}

		var cp = 0;
		var mp = 0;
		var ip = it;

		while (text.CharAt(it) != 0)
			if (pattern.CharAt(ip) == '*') {
				if (pattern.CharAt(++ip) == 0)
					return true;
				mp = ip;
				cp = it + 1;
			} else if (pattern.CharAt(ip) == text.CharAt(it) || pattern.CharAt(ip) == '?') {
				ip++;
				it++;
			} else {
				ip = mp;
				it = cp++;
			}

		while (pattern.CharAt(ip) == '*') ip++;
		return pattern.CharAt(ip) == 0;
	}

	/// <summary>
	///     Tronque une chaine de caractères à la longueur spécifiée, en remplaçant les derniers
	///     caractères par des points de suspension le cas échéant.
	/// </summary>
	/// <param name="s">La chaine à tronquer</param>
	/// <param name="maxLength">La longueur maximale souhaitée</param>
	/// <returns>La chaine tronquée</returns>
	public static string Ellipsis(this string s, int maxLength) {
		const string ellipsisString = "...";
		return s.Ellipsis(maxLength, ellipsisString);
	}

	/// <summary>
	///     Tronque une chaine de caractères à la longueur spécifiée, en remplaçant les derniers
	///     caractères par la chaine spécifiée le cas échéant.
	/// </summary>
	/// <param name="s">La chaine à tronquer</param>
	/// <param name="maxLength">La longueur maximale souhaitée</param>
	/// <param name="ellipsisString">La chaine à utiliser pour indiquer que la chaine est tronquée</param>
	/// <returns>La chaine tronquée</returns>
	public static string Ellipsis(this string s, int maxLength, string ellipsisString) =>
		s.Length <= maxLength ? s : s[..(maxLength - ellipsisString.Length)] + ellipsisString;

	/// <summary>
	///     Vérifie qu'une chaine se termine par le suffixe spécifié et l'ajoute si ce n'est pas le cas.
	/// </summary>
	/// <param name="s">Chaine originale</param>
	/// <param name="suffix">Suffixe à vérifier et éventuellement ajouter</param>
	/// <returns>La chaine originale si elle se termine par le le suffixe spécifié, sinon la chaine originale suivie du suffixe</returns>
	public static string EnsureEndsWith(this string s, string suffix) => !s.EndsWith(suffix) ? s + suffix : s;

	/// <summary>
	///     Vérifie qu'une chaine commence par le préfixe spécifié et l'ajoute si ce n'est pas le cas.
	/// </summary>
	/// <param name="s">Chaine originale</param>
	/// <param name="prefix">Préfixe à vérifier et éventuellement ajouter</param>
	/// <returns>La chaine originale si elle commence par le le préfixe spécifié, sinon la chaine originale précédée du préfixe</returns>
	public static string EnsureStartsWith(this string s, string prefix) => !s.StartsWith(prefix) ? prefix + s : s;


	/// <summary>
	///     Détermine si deux objets String ont la même valeur. Null et string.Empty sont considérés comme égaux
	/// </summary>
	/// <param name="baseString">chaîne servant de base à la comparaison</param>
	/// <param name="comparedString">chaîne à comparer</param>
	/// <returns>booléen indiquant si les deux chaînes sont égales</returns>
	public static bool EqualsWithNullAsEmpty(this string baseString, string comparedString) =>
		string.IsNullOrEmpty(baseString) ? string.IsNullOrEmpty(comparedString) : baseString.Equals(comparedString);

	/// <summary>
	///     Détermine si deux objets String ont la même valeur. Null et string.Empty sont considérés comme égaux
	/// </summary>
	/// <param name="baseString">chaîne servant de base à la comparaison</param>
	/// <param name="comparedString">chaîne à comparer</param>
	/// <param name="comparisonType">l'une des valeurs System.StringComparison</param>
	/// <returns>booléen indiquant si les deux chaînes sont égales</returns>
	public static bool EqualsWithNullAsEmpty(this string baseString,
	                                         string comparedString,
	                                         StringComparison comparisonType) => string.IsNullOrEmpty(baseString)
		? string.IsNullOrEmpty(comparedString)
		: baseString.Equals(comparedString, comparisonType);


	/// <summary>
	///     Remplace le caractère à la position spécifiée d'une chaine par le caractère spécifié.
	/// </summary>
	/// <param name="s">Chaine dans laquelle remplacer un caractère</param>
	/// <param name="index">Position à laquelle remplacer le caractère</param>
	/// <param name="newChar">Caractère de remplacement</param>
	/// <returns>La chaine modifiée.</returns>
	/// <remarks>
	///     Tout comme la méthode Replace, cette méthode ne modifie pas
	///     la chaine d'origine, mais renvoie une nouvelle chaine qui contient la
	///     modification.
	/// </remarks>
	public static string ReplaceAt(this string s, int index, char newChar) {
		var chars = s.ToCharArray();
		chars[index] = newChar;
		return new string(chars);
	}

	#region From/To/Take

	/// <summary>
	///     Renvoie une portion de chaine à partir de la position spécifiée
	/// </summary>
	/// <param name="s">Chaine dont on veut extraire une portion</param>
	/// <param name="start">Position de début de la portion</param>
	/// <returns>La portion de chaine demandée </returns>
	public static SubStringFrom From(this string s, int start) => new(s, start);

	/// <summary>
	///     Termine une portion de chaine à la position spécifiée
	/// </summary>
	/// <param name="subStringFrom">Portion de chaine à terminer</param>
	/// <param name="end">Position de fin</param>
	/// <returns>La portion de chaine demandée</returns>
	public static string To(this SubStringFrom subStringFrom, int end) =>
		subStringFrom.String.Substring(subStringFrom.Start, end - subStringFrom.Start + 1);

	/// <summary>
	///     Prend le nombre de caractères spécifié à partir du début de la portion
	/// </summary>
	/// <param name="subStringFrom">Portion de chaine à partir de laquelle prendre les caractères</param>
	/// <param name="count">Nombre de caractères à prendre</param>
	/// <returns>La portion de chaine demandée</returns>
	public static string Take(this SubStringFrom subStringFrom, int count) =>
		subStringFrom.String.Substring(subStringFrom.Start, count);

	/// <summary>
	///     Renvoie une portion de chaine à partir de la sous-chaine spécifiée, sans inclure cette dernière
	/// </summary>
	/// <param name="s">Chaine dont on veut extraire une portion</param>
	/// <param name="start">Sous-chaine de début de la portion</param>
	/// <returns>La portion de chaine demandée </returns>
	public static SubStringFrom From(this string s, string start) => s.From(start, false);

	/// <summary>
	///     Renvoie une portion de chaine à partir de la sous-chaine spécifiée, en incluant éventuellement cette dernière
	/// </summary>
	/// <param name="s">Chaine dont on veut extraire une portion</param>
	/// <param name="start">Sous-chaine de début de la portion</param>
	/// <param name="includeBoundary">true pour inclure la chaine de début spécifiée dans le résultat, false sinon</param>
	/// <returns>La portion de chaine demandée </returns>
	public static SubStringFrom From(this string s, string start, bool includeBoundary) {
		// ReSharper disable once StringIndexOfIsCultureSpecific.1
		var iStart = s.IndexOf(start);
		if (!includeBoundary && iStart >= 0)
			iStart += start.Length;
		return new SubStringFrom(s, iStart);
	}

	/// <summary>
	///     Termine une portion de chaine à la sous-chaine spécifiée, sans inclure cette dernière
	/// </summary>
	/// <param name="subStringFrom">Portion de chaine à terminer</param>
	/// <param name="end">Sous-chaine de fin</param>
	/// <returns>La portion de chaine demandée</returns>
	public static string To(this SubStringFrom subStringFrom, string end) => subStringFrom.To(end, false);

	/// <summary>
	///     Termine une portion de chaine à la sous-chaine spécifiée, en incluant éventuellement cette dernière
	/// </summary>
	/// <param name="subStringFrom">Portion de chaine à terminer</param>
	/// <param name="end">Sous-chaine de fin</param>
	/// <param name="includeBoundary">true pour inclure la chaine de fin spécifiée, false sinon</param>
	/// <returns>La portion de chaine demandée</returns>
	public static string To(this SubStringFrom subStringFrom, string end, bool includeBoundary) {
		var s = subStringFrom.String;
		var iStart = subStringFrom.Start;
		// ReSharper disable once StringIndexOfIsCultureSpecific.2
		var iEnd = s.IndexOf(end, iStart + 1);
		if (includeBoundary && iEnd > 0)
			iEnd += end.Length;
		if (iStart < 0)
			return string.Empty;
		return iEnd < 0 ? s[iStart..] : s[iStart..(iEnd - iStart)];
	}

	/// <summary>
	///     Représente une portion d'une chaine de caractères à partir d'une position donnée
	/// </summary>
	public struct SubStringFrom {
		internal SubStringFrom(string s, int start)
			: this() {
			String = s;
			Start = start;
		}

		/// <summary>
		///     La chaine dont cet objet représente une portion
		/// </summary>
		public string String { get; }

		/// <summary>
		///     La position de départ de la portion de chaine
		/// </summary>
		public int Start { get; }

		/// <summary>
		///     Convertit implicitement un SubStringForm en String
		/// </summary>
		/// <param name="subStringFrom">SubStringForm à convertir</param>
		/// <returns>La chaine correspondante</returns>
		public static implicit operator string(SubStringFrom subStringFrom) => subStringFrom.ToString();

		/// <summary>
		///     Renvoie une chaine équivalente à la portion de chaine
		/// </summary>
		/// <returns>Une chaine équivalente à la portion de chaine</returns>
		public override string ToString() => Start < 0 ? string.Empty : String[Start..];
	}

	#endregion From/To/Take

	/// <summary>
	/// </summary>
	/// <param name="count"></param>
	/// <returns></returns>
	public static string Spaces(int count) => new(' ', count);

	/// <summary>
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static bool IsNumeric(this string value) => decimal.TryParse(value, out _);

	/// <summary>
	///     Centers the input string (if it's longer than the length) and pads it using the padding string
	/// </summary>
	/// <param name="Input"></param>
	/// <param name="Length"></param>
	/// <param name="Padding"></param>
	/// <returns>The centered string</returns>
	public static string Center(this string Input, int Length, string Padding = " ") {
		if (string.IsNullOrEmpty(Input))
			Input = "";
		var Output = "";
		for (var x = 0; x < (Length - Input.Length) / 2; ++x) Output += Padding[x % Padding.Length];
		Output += Input;
		for (var x = 0; x < (Length - Input.Length) / 2; ++x) Output += Padding[x % Padding.Length];
		return Output;
	}

	private static readonly char[] _hexDigits = "0123456789abcdef".ToCharArray();

	/// <summary>
	///     Renvoie la représentation hexadécimale d'un tableau d'octets
	/// </summary>
	/// <param name="bytes">Un tableau d'octets</param>
	/// <returns>La représentation hexadécimale du tableau d'octets, avec des lettres minuscules et sans séparateur</returns>
	public static string ToHexString(this byte[] bytes) {
		var digits = new char[bytes.Length * 2];
		for (var i = 0; i < bytes.Length; i++) {
			var d1 = Math.DivRem(bytes[i], 16, out var d2);
			digits[2 * i] = _hexDigits[d1];
			digits[(2 * i) + 1] = _hexDigits[d2];
		}

		return new string(digits);
	}

	//[GeneratedRegex("^([\\w\\!\\#$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`{\\|\\}\\~]+\\.)*[\\w\\!\\#$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`{\\|\\}\\~]+@((((([a-z0-9]{1}[a-z0-9\\-]{0,62}[a-z0-9]{1})|[a-z])\\.)+[a-z]{2,6})|(\\d{1,3}\\.){3}\\d{1,3}(\\:\\d{1,5})?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "fr-FR")]
	//private static partial Regex MyRegex();
}