using Kay.Api.Exceptions;

namespace Kay.Api.Models
{
    public class Joueur
    {
        public const int LongueurMaximaleNom = 30;

        public int Id { get; }
        public string Nom { get; }
        public TypeJoueur Type { get; }
        // Rangée possédée : 0 pour le joueur 1 (trous 0..5), 1 pour le joueur 2 (trous 6..11)
        public int NumeroRangee { get; }
        public Joueur(int id, string nom, int numeroRangee, TypeJoueur type = TypeJoueur.Humain)
        {
            if (id is < 1 or > 2) throw new ArgumentOutOfRangeException(nameof(id));
            if (string.IsNullOrWhiteSpace(nom)) throw new ActionRefuseeException("Le nom du joueur est requis.");
            if (nom.Trim().Length > LongueurMaximaleNom)
                throw new ActionRefuseeException($"Le nom du joueur ne doit pas dépasser {LongueurMaximaleNom} caractères.");
            if (numeroRangee is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(numeroRangee));
            Id = id;
            Nom = nom.Trim();
            NumeroRangee = numeroRangee;
            Type = type;
        }
    }
}
