namespace CompteEstBon;

/// <summary>
///     status du tirage
/// </summary>
///  

public enum CebStatus:short {
    Indefini = 0,
    CompteEstBon = 1,
    CompteApproche = 2,
    Valide = 100,
    EnCours = 101,
    Invalide = -1
}