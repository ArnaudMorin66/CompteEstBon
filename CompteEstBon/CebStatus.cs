using System;

namespace CompteEstBon {

    /// <summary>
    /// status du tirage
    /// </summary>
    [Flags]
    public enum CebStatus {
        Indefini = 0,
        Valid = 1,
        EnCours = 2,
        CompteEstBon = 3,
        CompteApproche = 4,
        Erreur = 5
    }
}