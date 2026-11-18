using Kay.Api.Models;
using Kay.Api.Services;

namespace Kay.Tests.Outils
{
    /*
     Construit des parties dans un état précis et y joue des coups.
     Rangée 0 (J1) = positions 0..5, rangée 1 (J2) = positions 6..11.
    */
    internal static class Fabrique
    {
        // Trou désactivé dans un tableau de graines.
        public const int X = -1;

        public static Partie PartieEnCours(int[] cases, int receptacleJ1 = 0, int receptacleJ2 = 0, int joueurActif = 1,
            IRegles? regles = null)
        {
            var partie = new Partie("TEST", "A", regles ?? new ReglesKay(), ModePartie.EnLigne, Plateau.Depuis(cases, receptacleJ1, receptacleJ2));
            partie.AjouterAdversaire("B");
            partie.Demarrer(joueurActif);
            return partie;
        }

        public static void Jouer(Partie partie, int position) => partie.JouerCase(partie.JoueurActifId, position);

        // Les deux joueurs confirment le bilan : la manche suivante commence.
        public static void Continuer(Partie partie)
        {
            partie.CommencerMancheSuivante(1);
            partie.CommencerMancheSuivante(2);
        }

        public static int[] Graines(Partie partie) => Graines(partie.Plateau);

        public static int[] Graines(Plateau plateau) =>
            plateau.Cases.Select(c => c.EstActive ? c.NombreGraines : X).ToArray();

        public static int[] Receptacles(Partie partie) =>
            partie.Plateau.Receptacles.Select(r => r.NombreGraines).ToArray();

        // Graines du lot conservé (en main) de chaque rangée.
        public static int[] LotsConserves(Partie partie) =>
            [.. Enumerable.Range(0, 2).Select(r => partie.Plateau.ObtenirLotConserve(r)?.Graines ?? 0)];
    }
}
