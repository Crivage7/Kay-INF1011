using Kay.Api.Exceptions;
using Kay.Api.Models;
using Kay.Api.Services;
using static Kay.Tests.Outils.Fabrique;

namespace Kay.Tests
{
    // Partie nulle et variante « couper tête » 
    public sealed class NulleEtVariantesTests
    {
        // J1 commence la manche avec 21 graines (1 + 20), J2 avec 27 (8 + 19).
        // J1 joue 5 : sa graine complète le trou 6 (3 → 4), il capture 4 graines et sa rangée se vide.
        // Bilan : J1 = 20 + 4 = 24, J2 = 19 + 5 = 24 → J1 a récupéré ses graines.
        private static Partie PartieOuJ1RevientA24()
        {
            var partie = PartieEnCours([0, 0, 0, 0, 0, 1, 3, 1, 1, 1, 1, 1], receptacleJ1: 20, receptacleJ2: 19);
            Jouer(partie, 5);
            return partie;
        }

        [TestMethod]
        public void Nulle_JoueurRevenuA24_PeutDeclarerLaNulle()
        {
            var partie = PartieOuJ1RevientA24();
            Assert.AreEqual(EtatPartie.FinDeManche, partie.Etat);
            CollectionAssert.AreEqual(new[] { 24, 24 }, partie.DernierBilan!.Stocks.ToArray());
            Assert.AreEqual(1, partie.JoueurPouvantDeclarerNulle);

            Assert.ThrowsExactly<ActionRefuseeException>(() => partie.DeclarerNulle(2));
            partie.DeclarerNulle(1);
            Assert.AreEqual(EtatPartie.Terminee, partie.Etat);
            Assert.IsTrue(partie.EstNulle);
            Assert.IsNull(partie.VainqueurId);
        }

        [TestMethod]
        public void Nulle_LeJoueurPeutPrefererContinuer()
        {
            var partie = PartieOuJ1RevientA24();
            Continuer(partie);
            Assert.AreEqual(EtatPartie.EnCours, partie.Etat);
            Assert.IsNull(partie.JoueurPouvantDeclarerNulle);
            Assert.ThrowsExactly<ActionRefuseeException>(() => partie.DeclarerNulle(1));
        }

        [TestMethod]
        public void Nulle_ApresUneNulle_OnPeutRejouer()
        {
            var partie = PartieOuJ1RevientA24();
            partie.DeclarerNulle(1);
            partie.DemanderNouvellePartie(1);
            partie.DemanderNouvellePartie(2);
            Assert.AreEqual(EtatPartie.EnCours, partie.Etat);
            Assert.IsFalse(partie.EstNulle);
        }

        [TestMethod]
        public void Nulle_PasProposeeSansEgalite()
        {
            // Bilan 20 / 26 : personne n'a récupéré toutes ses graines.
            var partie = PartieEnCours([0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1], receptacleJ1: 20, receptacleJ2: 20);
            Jouer(partie, 5);
            Assert.IsNull(partie.JoueurPouvantDeclarerNulle);
        }

        [TestMethod]
        public void Nulle_PasProposeeSiPersonneNAvaitPerduDeGraines()
        {
            var bilan = new BilanManche(1, [0, 0], [24, 24], [6, 6], [0, 0]);
            Assert.IsNull(new ReglesKay().RangeePouvantDeclarerNulle([24, 24], bilan));
            Assert.AreEqual(1, new ReglesKay().RangeePouvantDeclarerNulle([28, 20], bilan));
        }

        [TestMethod]
        public void CouperLaTete_TroisTrousDesactives_LaPartieContinue()
        {
            var partie = PartieEnCours([0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1], receptacleJ1: 8, receptacleJ2: 30,
                regles: new ReglesKayCouperLaTete());
            Jouer(partie, 5);
            Assert.AreEqual(EtatPartie.FinDeManche, partie.Etat);
            Assert.AreEqual(VarianteRegles.CouperLaTete, partie.Variante);
        }

        [TestMethod]
        public void CouperLaTete_PlusAucunTrouRemplissable_LaPartieEstPerdue()
        {
            // J1 n'a plus que 2 graines : il ne peut remplir aucun trou, J2 lui a « coupé la tête ».
            var partie = PartieEnCours([0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1], receptacleJ1: 2, receptacleJ2: 30,
                regles: new ReglesKayCouperLaTete());
            Jouer(partie, 5);
            Assert.AreEqual(EtatPartie.Terminee, partie.Etat);
            Assert.AreEqual(2, partie.VainqueurId);
        }

        [TestMethod]
        public void Classique_EstLaVarianteParDefaut() =>
            Assert.AreEqual(VarianteRegles.Classique, PartieEnCours([4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4]).Variante);
    }
}
