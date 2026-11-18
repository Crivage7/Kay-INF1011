using Kay.Api.Exceptions;
using Kay.Api.Models;
using Kay.Api.Services;
using static Kay.Tests.Outils.Fabrique;

namespace Kay.Tests
{
    // Propriétés vraies pour n'importe quelle suite de coups valides
    [TestClass]
    public sealed class InvariantTests
    {
        private const int NombreParties = 300;
        private const int CoupsMaximum = 5000;

        [TestMethod]
        [DataRow(VarianteRegles.Classique)]
        [DataRow(VarianteRegles.CouperLaTete)]
        public void TotalDesGraines_ToujoursQuaranteHuit_SurDesPartiesAleatoires(VarianteRegles variante)
        {
            IRegles regles = variante == VarianteRegles.Classique ? new ReglesKay() : new ReglesKayCouperLaTete();
            var hasard = new Random(1011);
            int terminees = 0;
            for (int n = 0; n < NombreParties; n++)
            {
                var partie = PartieEnCours([4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4], regles: regles);
                for (int coup = 0; coup < CoupsMaximum && !partie.EstTerminee; coup++)
                {
                    if (partie.Etat == EtatPartie.FinDeManche) Continuer(partie);
                    int rangee = partie.JoueurActifId - 1;
                    var choix = partie.Plateau.Cases
                        .Where(c => c.Position / 6 == rangee && c.EstActive && c.NombreGraines > 0)
                        .Select(c => c.Position)
                        .OrderBy(_ => hasard.Next())
                        .ToList();
                    // Au premier tour, une partie sur deux prend deux lots (2.1).
                    int? conservee = partie.DeuxLotsPermis && choix.Count > 1 && hasard.Next(2) == 0 ? choix[^1] : null;
                    // Un relais sans fin est refusé sans modifier la partie : on essaie le trou suivant.
                    bool joue = choix.Any(position =>
                    {
                        try { partie.JouerCase(partie.JoueurActifId, position, position == conservee ? null : conservee); return true; }
                        catch (CoupInvalideException) { return false; }
                    });
                    Assert.IsTrue(joue, $"Partie {n}, coup {coup} : le joueur actif doit avoir un coup jouable.");
                    // Le lot conservé (en main) fait partie des graines en jeu.
                    Assert.AreEqual(48, Graines(partie).Where(g => g > 0).Sum() + Receptacles(partie).Sum() + LotsConserves(partie).Sum(),
                        $"Partie {n}, coup {coup}");
                    // Le lot conservé ne survit pas au tour du joueur qui l'a pris (2.1).
                    if (partie.Etat != EtatPartie.EnCours) Assert.AreEqual(0, LotsConserves(partie).Sum(), $"Partie {n}, coup {coup}");
                    else Assert.AreEqual(0, LotsConserves(partie)[2 - partie.JoueurActifId], $"Partie {n}, coup {coup}");
                }
                if (partie.Etat == EtatPartie.Terminee) terminees++;
            }
            Assert.AreEqual(NombreParties, terminees, "Toutes les parties simulées doivent se terminer.");
        }
    }
}
