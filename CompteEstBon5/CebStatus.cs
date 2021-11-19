using System;

namespace CompteEstBon {

    /// <summary>
    /// status du tirage
    /// </summary>
   
    public enum CebStatus {
        Indefini,
        Valide,
        EnCours,
        CompteEstBon,
        CompteApproche,
        Invalide
    }
}