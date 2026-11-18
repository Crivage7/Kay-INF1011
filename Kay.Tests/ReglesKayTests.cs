using Kay.Api.Exceptions;
using Kay.Api.Models;
using Kay.Api.Services;
using static Kay.Tests.Outils.Fabrique;

namespace Kay.Tests
{
    
    // Règles de distribution et de capture 
    // Les plateaux sont construits à la main ; positions 0..5 = J1 (sème vers +1), 6..11 = J2 (sème vers -1).
    
    [TestClass]
    public sealed class ReglesKayTests
    {
        //                                   J1 : 0  1  2  3  4  5 | J2 : 6  7  8  9 10 11
        private static readonly int[] RangeeJ2Garnie = [2, 2, 2, 2, 2, 2];

        private static int[] Plateau(int[] rangeeJ1, int[]? rangeeJ2 = null) => [.. rangeeJ1, .. rangeeJ2 ?? RangeeJ2Garnie];

        [TestMethod]
        public void Distribution_DerniereGraineDansTrouVide_JoueurMortEtTourPasse()
        {
            var partie = PartieEnCours(Plateau([1, 0, 0, 0, 0, 3]));
            Jouer(partie, 0);
            CollectionAssert.AreEqual(Plateau([0, 1, 0, 0, 0, 3]), Graines(partie));
            Assert.AreEqual(2, partie.JoueurActifId);
        }

        [TestMethod]
        public void Distribution_TrouDArriveeOccupe_JoueurRamasseEtContinue()
        {
            // 0 → 1 (devient 2, ni vide ni 3→4) : on reprend 2 graines → 2, 3 ; le trou 3 était vide.
            var partie = PartieEnCours(Plateau([1, 1, 0, 0, 0, 3]));
            Jouer(partie, 0);
            CollectionAssert.AreEqual(Plateau([0, 0, 1, 1, 0, 3]), Graines(partie));
            Assert.AreEqual(2, partie.JoueurActifId);
        }

        [TestMethod]
        public void Distribution_TrouDesactive_EstIgnore()
        {
            var partie = PartieEnCours(Plateau([2, X, 0, 0, 0, 3]));
            Jouer(partie, 0);
            CollectionAssert.AreEqual(Plateau([0, X, 1, 1, 0, 3]), Graines(partie));
        }

        [TestMethod]
        public void Distribution_J1_PasseDeSaRangeeALaRangeeAdverse()
        {
            // 7 graines depuis 5 : 6, 7, 8, 9, 10, 11 puis 0 (vide → mort).
            var partie = PartieEnCours(Plateau([0, 0, 0, 0, 1, 7], [0, 0, 0, 0, 0, 0]));
            Jouer(partie, 5);
            CollectionAssert.AreEqual(Plateau([1, 0, 0, 0, 1, 0], [1, 1, 1, 1, 1, 1]), Graines(partie));
        }

        [TestMethod]
        public void Distribution_J2_SemeEnSensInverseEtPasseALaRangeeDeJ1()
        {
            // J2 sème vers -1 : depuis 6, la graine tombe dans le trou 5 (rangée de J1).
            var partie = PartieEnCours(Plateau([2, 2, 2, 2, 2, 0], [1, 0, 0, 0, 0, 3]), joueurActif: 2);
            Jouer(partie, 6);
            CollectionAssert.AreEqual(Plateau([2, 2, 2, 2, 2, 1], [0, 0, 0, 0, 0, 3]), Graines(partie));
            Assert.AreEqual(1, partie.JoueurActifId);
        }

        [TestMethod]
        public void Capture_TroisDevientQuatre_GrainesAuReceptacleEtJoueurRejoue()
        {
            var partie = PartieEnCours(Plateau([1, 3, 0, 0, 0, 2]));
            Jouer(partie, 0);
            CollectionAssert.AreEqual(Plateau([0, 0, 0, 0, 0, 2]), Graines(partie));
            CollectionAssert.AreEqual(new[] { 4, 0 }, Receptacles(partie));
            Assert.AreEqual(1, partie.JoueurActifId);
        }

        [TestMethod]
        public void Capture_DansLeCampAdverse()
        {
            var partie = PartieEnCours(Plateau([2, 0, 0, 0, 0, 1], [3, 2, 2, 2, 2, 2]));
            Jouer(partie, 5);
            CollectionAssert.AreEqual(Plateau([2, 0, 0, 0, 0, 0], [0, 2, 2, 2, 2, 2]), Graines(partie));
            CollectionAssert.AreEqual(new[] { 4, 0 }, Receptacles(partie));
        }

        [TestMethod]
        public void Capture_EnChaine_TrousPrecedentsAQuatre()
        {
            // 2 graines depuis 0 : 1 (3→4) puis 2 (3→4) → capture de 2 et, en chaîne, de 1.
            var partie = PartieEnCours(Plateau([2, 3, 3, 0, 0, 1]));
            Jouer(partie, 0);
            CollectionAssert.AreEqual(Plateau([0, 0, 0, 0, 0, 1]), Graines(partie));
            CollectionAssert.AreEqual(new[] { 8, 0 }, Receptacles(partie));
        }

        [TestMethod]
        public void Capture_EnChaine_SArreteAuPremierTrouDifferentDeQuatre()
        {
            // 3 graines depuis 0 : 1 (2→3), 2 (3→4), 3 (3→4) → capture de 3 et 2, mais pas de 1.
            var partie = PartieEnCours(Plateau([3, 2, 3, 3, 0, 1]));
            Jouer(partie, 0);
            CollectionAssert.AreEqual(Plateau([0, 3, 0, 0, 0, 1]), Graines(partie));
            CollectionAssert.AreEqual(new[] { 8, 0 }, Receptacles(partie));
        }

        [TestMethod]
        public void Capture_EnChaine_IgnoreLesTrousDesactives()
        {
            // Le trou désactivé 2 est sauté, la chaîne continue au trou actif précédent.
            // 3 graines depuis 0 : 1 (3→4), 3 (3→4), 4 (3→4) ; capture de 4, 3, puis 1 en sautant 2.
            var partie = PartieEnCours(Plateau([3, 3, X, 3, 3, 1]));
            Jouer(partie, 0);
            CollectionAssert.AreEqual(Plateau([0, 0, X, 0, 0, 1]), Graines(partie));
            CollectionAssert.AreEqual(new[] { 12, 0 }, Receptacles(partie));
        }

        [TestMethod]
        [DataRow(1, DisplayName = "trou vide")]
        [DataRow(2, DisplayName = "trou désactivé")]
        [DataRow(7, DisplayName = "rangée adverse")]
        public void CoupInvalide_EstRefuseSansModifierLePlateau(int position)
        {
            int[] depart = Plateau([4, 0, X, 4, 4, 4]);
            var partie = PartieEnCours(depart);
            Assert.ThrowsExactly<CoupInvalideException>(() => Jouer(partie, position));
            CollectionAssert.AreEqual(depart, Graines(partie));
            Assert.AreEqual(1, partie.JoueurActifId);
        }

        [TestMethod]
        public void RelaisSansFin_CoupRefuseEtPlateauIntact()
        {
            // Position trouvée par simulation : depuis le trou 8, le relais de J2 repasse indéfiniment par les mêmes états.
            int[] depart = [1, 0, 1, 2, 8, 4, 2, 1, 3, 1, 5, 8];
            var partie = PartieEnCours(depart, joueurActif: 2);
            var erreur = Assert.ThrowsExactly<CoupInvalideException>(() => Jouer(partie, 8));
            StringAssert.Contains(erreur.Message, "ne se termine jamais");
            CollectionAssert.AreEqual(depart, Graines(partie));
            Assert.AreEqual(2, partie.JoueurActifId);
        }

        [TestMethod]
        public void ChoixAutorise_DonneUnMotifExplicatif()
        {
            var regles = new ReglesKay();
            var plateau = Kay.Api.Models.Plateau.Depuis([4, 0, X, 4, 4, 4, 4, 4, 4, 4, 4, 4]);
            var joueur1 = new Joueur(1, "A", 0);
            Assert.IsTrue(regles.ChoixAutorise(plateau, joueur1, 0).EstAutorise);
            Assert.AreEqual("Ce trou est vide.", regles.ChoixAutorise(plateau, joueur1, 1).Motif);
            Assert.AreEqual("Ce trou est désactivé pour cette manche.", regles.ChoixAutorise(plateau, joueur1, 2).Motif);
            Assert.AreEqual("Ce trou n'appartient pas à votre rangée.", regles.ChoixAutorise(plateau, joueur1, 6).Motif);
            Assert.AreEqual("Ce trou n'existe pas.", regles.ChoixAutorise(plateau, joueur1, 12).Motif);
        }

        [TestMethod]
        public void DeterminerVainqueur_DeuxTrousDesactives_LaPartieContinue()
        {
            var plateau = Kay.Api.Models.Plateau.Depuis([X, X, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4]);
            Assert.IsNull(new ReglesKay().DeterminerVainqueur(plateau));
        }
    }
}
