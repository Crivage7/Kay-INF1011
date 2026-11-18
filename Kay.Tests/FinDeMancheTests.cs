using Kay.Api.Models;
using static Kay.Tests.Outils.Fabrique;

namespace Kay.Tests
{
    // Fin de manche, redistribution et victoire
    [TestClass]
    public sealed class FinDeMancheTests
    {
        [TestMethod]
        public void RangeeVide_AdversaireRamasseSaRangeeEtChacunRedistribue()
        {
            // J1 joue sa dernière graine : sa rangée est vide, J2 ramasse ses 6 graines (20 + 6 = 26).
            var partie = PartieEnCours([0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1], receptacleJ1: 20, receptacleJ2: 20);
            Jouer(partie, 5);

            // J1 : 20 graines → 5 trous, le 1er trou de sa rangée est désactivé. J2 : 26 → 6 trous, 2 en réserve.
            CollectionAssert.AreEqual(new[] { X, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 }, Graines(partie));
            CollectionAssert.AreEqual(new[] { 0, 2 }, Receptacles(partie));
            Assert.AreEqual(EtatPartie.FinDeManche, partie.Etat);
        }

        [TestMethod]
        public void TroisTrousDesactivesOuPlus_LaPartieEstPerdue()
        {
            var partie = PartieEnCours([0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1], receptacleJ1: 8, receptacleJ2: 30);
            Jouer(partie, 5);

            CollectionAssert.AreEqual(new[] { X, X, X, X, 4, 4, 4, 4, 4, 4, 4, 4 }, Graines(partie));
            CollectionAssert.AreEqual(new[] { 0, 12 }, Receptacles(partie));
            Assert.AreEqual(EtatPartie.Terminee, partie.Etat);
            Assert.AreEqual(2, partie.VainqueurId);
        }

        [TestMethod]
        public void CaptureQuiVideLaRangee_LaFinDeMancheEstPrioritaire()
        {
            // J1 capture ses 4 dernières graines : il garderait la main, mais sa rangée est vide.
            var partie = PartieEnCours([1, 3, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2], receptacleJ1: 16, receptacleJ2: 10);
            Jouer(partie, 0);

            // J1 : 16 + 4 = 20 → 5 trous. J2 : 10 + 12 = 22 → 5 trous, 2 en réserve.
            CollectionAssert.AreEqual(new[] { X, 4, 4, 4, 4, 4, X, 4, 4, 4, 4, 4 }, Graines(partie));
            CollectionAssert.AreEqual(new[] { 0, 2 }, Receptacles(partie));
            Assert.AreEqual(EtatPartie.FinDeManche, partie.Etat);
        }

        [TestMethod]
        public void Redistribution_TrouReactiveSiLeStockLePermet()
        {
            // un stock qui passe de 16 à 20 graines réactive le trou 2, le trou 1 reste désactivé.
            var partie = PartieEnCours([X, X, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1], receptacleJ1: 20, receptacleJ2: 20);
            Jouer(partie, 5);
            CollectionAssert.AreEqual(new[] { X, 4, 4, 4, 4, 4 }, Graines(partie)[..6]);
        }

        [TestMethod]
        public void Redistribution_J2_DesactiveAPartirDuTrouASaGauche()
        {
            // Le premier trou de J2 est la position 6 (à sa gauche, vu de sa place).
            // J2 joue sa dernière graine (6 → 5) ; J1 ramasse ses graines. J2 : 16 graines → 4 trous (8..11).
            var partie = PartieEnCours([1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0], receptacleJ1: 22, receptacleJ2: 16, joueurActif: 2);
            Jouer(partie, 6);
            CollectionAssert.AreEqual(new[] { X, X, 4, 4, 4, 4 }, Graines(partie)[6..]);
        }

        [TestMethod]
        public void MancheSuivante_J1AvaitCommence_J2Commence()
        {
            // alternance ; celui qui n'a pas commencé la manche précédente commence.
            var partie = PartieEnCours([0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1], receptacleJ1: 20, receptacleJ2: 20, joueurActif: 1);
            Jouer(partie, 5);
            Continuer(partie);
            Assert.AreEqual(2, partie.NumeroManche);
            Assert.AreEqual(2, partie.JoueurActifId);
        }

        [TestMethod]
        public void MancheSuivante_J2AvaitCommence_J1Commence()
        {
            var partie = PartieEnCours([1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0], receptacleJ1: 22, receptacleJ2: 16, joueurActif: 2);
            Jouer(partie, 6);
            Continuer(partie);
            Assert.AreEqual(2, partie.NumeroManche);
            Assert.AreEqual(1, partie.JoueurActifId);
        }

        [TestMethod]
        public void FinDeManche_BilanDisponible()
        {
            var partie = PartieEnCours([0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1], receptacleJ1: 20, receptacleJ2: 20);
            Jouer(partie, 5);

            var bilan = partie.DernierBilan!;
            Assert.AreEqual(1, bilan.NumeroManche);
            CollectionAssert.AreEqual(new[] { 0, 6 }, bilan.GrainesRamassees.ToArray());
            CollectionAssert.AreEqual(new[] { 20, 26 }, bilan.Stocks.ToArray());
            CollectionAssert.AreEqual(new[] { 5, 6 }, bilan.TrousActifs.ToArray());
            CollectionAssert.AreEqual(new[] { 0, 2 }, bilan.Reserves.ToArray());
        }
    }
}
