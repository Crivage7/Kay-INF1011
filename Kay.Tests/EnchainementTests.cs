using Kay.Api.Exceptions;
using Kay.Api.Models;
using static Kay.Tests.Outils.Fabrique;

namespace Kay.Tests
{
    // Bilan de manche, manche suivante et nouvelle partie
    [TestClass]
    public sealed class EnchainementTests
    {
        // J1 joue 5 : sa rangée se vide, la manche 1 se termine sans vainqueur.
        private static Partie PartieEnFinDeManche()
        {
            var partie = PartieEnCours([0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1], receptacleJ1: 20, receptacleJ2: 20);
            Jouer(partie, 5);
            return partie;
        }

        // J1 joue 5 avec seulement 8 graines en réserve : J2 gagne.
        private static Partie PartieTerminee()
        {
            var partie = PartieEnCours([0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1], receptacleJ1: 8, receptacleJ2: 30);
            Jouer(partie, 5);
            return partie;
        }

        [TestMethod]
        public void FinDeManche_LaPartieAttendLeBilanAvantLaMancheSuivante()
        {
            var partie = PartieEnFinDeManche();
            Assert.AreEqual(EtatPartie.FinDeManche, partie.Etat);
            Assert.AreEqual(1, partie.NumeroManche);
            Assert.IsNotNull(partie.DernierBilan);
            Assert.ThrowsExactly<ActionRefuseeException>(() => partie.JouerCase(partie.JoueurActifId, 1));
        }

        [TestMethod]
        public void MancheSuivante_DemarreQuandLesDeuxJoueursOntConfirme()
        {
            var partie = PartieEnFinDeManche();
            partie.CommencerMancheSuivante(1);
            partie.CommencerMancheSuivante(1);
            Assert.AreEqual(EtatPartie.FinDeManche, partie.Etat, "Un seul joueur a confirmé.");
            CollectionAssert.AreEquivalent(new[] { 1 }, partie.Confirmations.ToArray());

            partie.CommencerMancheSuivante(2);
            Assert.AreEqual(EtatPartie.EnCours, partie.Etat);
            Assert.AreEqual(2, partie.NumeroManche);
            Assert.IsEmpty(partie.Confirmations);
        }

        [TestMethod]
        public void MancheSuivante_PendantUneManche_EstRefusee()
        {
            var partie = PartieEnCours([4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4]);
            Assert.ThrowsExactly<ActionRefuseeException>(() => partie.CommencerMancheSuivante(1));
        }

        [TestMethod]
        public void Victoire_BilanDisponibleEtPartieTerminee()
        {
            var partie = PartieTerminee();
            Assert.AreEqual(EtatPartie.Terminee, partie.Etat);
            Assert.IsTrue(partie.EstTerminee);
            Assert.AreEqual(2, partie.VainqueurId);
            Assert.IsNotNull(partie.DernierBilan);
            Assert.ThrowsExactly<ActionRefuseeException>(() => partie.CommencerMancheSuivante(1));
        }

        [TestMethod]
        public void NouvellePartie_ReinitialiseLePlateauQuandLesDeuxJoueursLeDemandent()
        {
            var partie = PartieTerminee();
            partie.DemanderNouvellePartie(2);
            Assert.AreEqual(EtatPartie.Terminee, partie.Etat, "Un seul joueur a demandé à rejouer.");

            partie.DemanderNouvellePartie(1);
            Assert.AreEqual(EtatPartie.EnCours, partie.Etat);
            Assert.AreEqual(1, partie.NumeroManche);
            Assert.IsNull(partie.VainqueurId);
            Assert.IsNull(partie.DernierBilan);
            Assert.IsTrue(partie.Plateau.Cases.All(c => c.EstActive && c.NombreGraines == 4));
            CollectionAssert.AreEqual(new[] { 0, 0 }, Receptacles(partie));
            Assert.AreEqual("A", partie.Joueur1.Nom, "Les joueurs sont conservés.");
            Assert.AreEqual("B", partie.Joueur2!.Nom);
        }

        [TestMethod]
        public void NouvellePartie_PendantLaPartie_EstRefusee()
        {
            var partie = PartieEnCours([4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4]);
            Assert.ThrowsExactly<ActionRefuseeException>(() => partie.DemanderNouvellePartie(1));
        }

        [TestMethod]
        public void Demarrer_SansArgument_TireAuSortLePremierJoueur()
        {
            var premiers = new HashSet<int>();
            for (int graine = 0; graine < 20; graine++)
            {
                var partie = new Partie("TEST", "A", new Kay.Api.Services.ReglesKay(), ModePartie.EnLigne, new Random(graine));
                partie.AjouterAdversaire("B");
                partie.Demarrer();
                premiers.Add(partie.JoueurActifId);
            }
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, premiers.ToArray());
        }
    }
}
