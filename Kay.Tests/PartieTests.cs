using Kay.Api.Exceptions;
using Kay.Api.Models;
using Kay.Api.Services;
using static Kay.Tests.Outils.Fabrique;

namespace Kay.Tests
{
    // Partie en tant que contrôleur : enchaînement des états et des tours
    [TestClass]
    public sealed class PartieTests
    {
        private static Partie NouvellePartie() => new("TEST", "Alice", new ReglesKay());

        [TestMethod]
        public void NouvellePartie_EnAttenteDuDeuxiemeJoueur()
        {
            var partie = NouvellePartie();
            Assert.AreEqual(EtatPartie.EnAttente, partie.Etat);
            Assert.AreEqual(ModePartie.EnLigne, partie.Mode);
            Assert.AreEqual(TypeJoueur.Humain, partie.Joueur1.Type);
            Assert.IsNull(partie.Joueur2);
        }

        [TestMethod]
        public void Demarrer_DonneLaMainAuPremierJoueur()
        {
            var partie = NouvellePartie();
            partie.AjouterAdversaire("Bob");
            partie.Demarrer(2);
            Assert.AreEqual(EtatPartie.EnCours, partie.Etat);
            Assert.AreEqual(2, partie.JoueurActifId);
            Assert.AreEqual(1, partie.NumeroManche);
        }

        [TestMethod]
        public void Demarrer_SansAdversaire_EstRefuse() =>
            Assert.ThrowsExactly<ActionRefuseeException>(() => NouvellePartie().Demarrer(1));

        [TestMethod]
        public void AjouterAdversaire_PartieComplete_EstRefuse()
        {
            var partie = NouvellePartie();
            partie.AjouterAdversaire("Bob");
            Assert.ThrowsExactly<ActionRefuseeException>(() => partie.AjouterAdversaire("Charlie"));
        }

        [TestMethod]
        public void JouerCase_AvantLeDemarrage_EstRefuse() =>
            Assert.ThrowsExactly<ActionRefuseeException>(() => NouvellePartie().JouerCase(1, 0));

        [TestMethod]
        public void JouerCase_HorsTour_EstRefuse()
        {
            var partie = PartieEnCours([4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4], joueurActif: 1);
            var erreur = Assert.ThrowsExactly<CoupInvalideException>(() => partie.JouerCase(2, 6));
            Assert.AreEqual("Ce n'est pas votre tour.", erreur.Message);
        }

        [TestMethod]
        public void JouerCase_AjouteUneLigneAHistorique()
        {
            var partie = PartieEnCours([1, 0, 0, 0, 0, 3, 2, 2, 2, 2, 2, 2]);
            int avant = partie.Historique.Count;
            Jouer(partie, 0);
            Assert.HasCount(avant + 1, partie.Historique);
            StringAssert.Contains(partie.Historique[^1], "trou 0");
        }

        [TestMethod]
        public void Statistiques_CoupsEtCapturesParJoueur()
        {
            // J1 capture 4 graines (et rejoue), puis joue un coup sans capture ; la main passe à J2.
            var partie = PartieEnCours([1, 3, 0, 0, 1, 2, 2, 2, 2, 2, 2, 2]);
            Jouer(partie, 0);
            Jouer(partie, 4);
            CollectionAssert.AreEqual(new[] { 2, 0 }, partie.CoupsJoues.ToArray());
            CollectionAssert.AreEqual(new[] { 4, 0 }, partie.GrainesCapturees.ToArray());
        }

        [TestMethod]
        public void Statistiques_RemisesAZeroPourUneNouvellePartie()
        {
            var partie = PartieEnCours([0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1], receptacleJ1: 8, receptacleJ2: 30);
            Jouer(partie, 5);
            partie.DemanderNouvellePartie(1);
            partie.DemanderNouvellePartie(2);
            CollectionAssert.AreEqual(new[] { 0, 0 }, partie.CoupsJoues.ToArray());
            CollectionAssert.AreEqual(new[] { 0, 0 }, partie.GrainesCapturees.ToArray());
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("Un nom beaucoup trop long pour être affiché")]
        public void NomDeJoueurInvalide_EstRefuse(string nom) =>
            Assert.ThrowsExactly<ActionRefuseeException>(() => new Partie("TEST", nom, new ReglesKay()));
    }
}
