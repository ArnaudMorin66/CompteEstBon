using System.Diagnostics;

namespace arnaud.morin.outils;

public static class Outils {
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static IEnumerable<(T, int)> WithIndex<T>(this IEnumerable<T> self) =>
        self.Select((value, index) => (value, index));

    /// <summary>
    ///     Teste si une valeur est entre les 2 valeurs spécifiées (bornes inclues)
    /// </summary>
    /// <typeparam name="T">Type de la valeur</typeparam>
    /// <param name="value">Valeur à tester</param>
    /// <param name="min">Borne inférieure</param>
    /// <param name="max">Borne supérieure</param>
    /// <returns>true si <c>min &lt;= value &lt;= max</c>, sinon false</returns>
    public static bool Between<T>(this T value, T min, T max)
        where T : IComparable<T> => min.CompareTo(value) <= 0 && value.CompareTo(max) <= 0;

    /// <summary>
    ///     Teste si une valeur est strictement entre les bornes spécifiées (bornes non inclues)
    /// </summary>
    /// <typeparam name="T">Type de la valeur</typeparam>
    /// <param name="value">Valeur à tester</param>
    /// <param name="min">Borne inférieure</param>
    /// <param name="max">Borne supérieure</param>
    /// <returns>true si <c>min &lt; value &lt; max</c>, sinon false</returns>
    public static bool StrictlyBetween<T>(this T value, T min, T max)
        where T : IComparable<T> => min.CompareTo(value) < 0 && value.CompareTo(max) < 0;

    /// <summary>
    /// </summary>
    /// <param name="nom"></param>
    public static void OpenDocument(string nom) =>
        Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = nom });
}