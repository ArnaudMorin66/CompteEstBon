using System.ComponentModel;

namespace CompteEstBon;

/// <summary>
/// Représente un tirage pour le jeu "Compte est bon".
/// Hérite de <see cref="CebTirageBase"/> et implémente <see cref="INotifyPropertyChanged"/>.
/// </summary>
public class CebTirage : CebTirageBase, INotifyPropertyChanged {
    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="CebTirage"/> avec un nombre de plaques spécifié.
    /// </summary>
    /// <param name="n">Nombre de plaques du tirage. La valeur par défaut est 6.</param>
    public CebTirage(int n = 6) : base(n) {
    }

    /// <summary>
    /// Événement déclenché lorsqu'une propriété change.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Déclenche l'événement <see cref="PropertyChanged"/> pour une propriété spécifiée.
    /// </summary>
    /// <param name="propertyName">Nom de la propriété qui a changé.</param>
    protected void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Réinitialise les données du tirage.
    /// </summary>
    /// <returns>
    /// Le statut de l'opération de réinitialisation.
    /// </returns>
    public override CebStatus Clear() {
        var ret = base.Clear();
        OnPropertyChanged(nameof(Clear));
        return ret;
    }

    /// <summary>
    /// Résout le problème du "Compte est bon".
    /// </summary>
    /// <returns>Le statut de l'opération de résolution.</returns>
    public override CebStatus Solve() {
        var ret = base.Solve();
        OnPropertyChanged(nameof(Solve));
        return ret;
    }
}
