using System.CommandLine.Rendering;

namespace arnaud.morin.outils;
public static class AnsiUtils {
    /// <summary>
    ///
    /// </summary>


    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <param name="color"></param>
    /// <param name="resetColor"></param>
    /// <returns></returns>
    public static string ControlCode(this string texte, AnsiControlCode color, AnsiControlCode? resetColor = null) => $"{color}{texte}{resetColor ?? Ansi.Color.Foreground.Default}";

    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Red(this string texte) => texte.ControlCode(Ansi.Color.Foreground.Red);

    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string LightYellow(this string texte) => texte.ControlCode(Ansi.Color.Foreground.LightYellow);

    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Cyan(this string texte) => texte.ControlCode(Ansi.Color.Foreground.Cyan);

    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Green(this string texte) => texte.ControlCode(Ansi.Color.Foreground.Green);

    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Magenta(this string texte) => texte.ControlCode(Ansi.Color.Foreground.Magenta);

    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Yellow(this string texte) => texte.ControlCode(Ansi.Color.Foreground.Yellow);

    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Blue(this string texte) => texte.ControlCode(Ansi.Color.Foreground.Blue);

    public static string Underline(this string texte) => texte.ControlCode(Ansi.Text.UnderlinedOn, Ansi.Text.AttributesOff);
    public static string Bold(this string texte) => texte.ControlCode(Ansi.Text.BoldOn, Ansi.Text.BoldOff);
    public static string ReverseColors(this string texte) => texte.ControlCode(Ansi.Text.ReverseOn, Ansi.Text.ReverseOff);

}