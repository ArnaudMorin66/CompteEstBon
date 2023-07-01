//-----------------------------------------------------------------------
// <copyright file="CebUtilitaires.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.CommandLine.Rendering;

namespace CompteEstBon;

// ReSharper disable once CheckNamespace
/// <summary>
///
/// </summary>
public static class CebUtilitaires {
    /// <summary>
    ///
    /// </summary>


    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <param name="bground"></param>
    /// <param name="eground"></param>
    /// <returns></returns>
    public static string ControlCode(this object texte, AnsiControlCode bground, AnsiControlCode eground = null) => $"{bground}{texte}{eground ?? Ansi.Color.Foreground.Default}";

    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Red(this object texte) => texte.ControlCode(Ansi.Color.Foreground.Red);

    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string LightYellow(this object texte) => texte.ControlCode(Ansi.Color.Foreground.LightYellow);

    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Cyan(this object texte) => texte.ControlCode(Ansi.Color.Foreground.Cyan);

    /// <summary>
///
/// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
        public static string Green(this object texte) => texte.ControlCode(Ansi.Color.Foreground.Green);

    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Magenta(this object texte) => texte.ControlCode(Ansi.Color.Foreground.Magenta);

    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Yellow(this object texte) => texte.ControlCode(Ansi.Color.Foreground.Yellow);

    /// <summary>
    ///
    /// </summary>
    /// <param name="texte"></param>
    /// <returns></returns>
    public static string Blue(this object texte) => texte.ControlCode(Ansi.Color.Foreground.Blue);
}