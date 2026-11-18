using Kay.Api.Exceptions;
using Kay.Api.Models;
using Kay.Api.Services;
using Kay.Tests.Outils;

namespace Kay.Tests
{
    [TestClass]
    public sealed class PartieServiceTests
    {
        private static PartieService NouveauService() => new([new ReglesKay(), new ReglesKayCouperLaTete()], new DepotResultatsMemoire());

        [TestMethod]
        public void Rejoindre_DemarreLaPartie()
        {
            var service = NouveauService();
            var a = service.Creer("Alice");
            var b = service.Rejoindre(a.Partie.Code.ToLowerInvariant(), "Bob");
            Assert.AreEqual(Kay.Api.Models.EtatPartie.EnCours, b.Partie.Etat);
            Assert.AreEqual(2, b.JoueurId);
            Assert.AreNotEqual(a.Jeton, b.Jeton);
        }

        [TestMethod]
        public void Creer_AvecLaVarianteChoisie()
        {
            var service = NouveauService();
            Assert.AreEqual(Kay.Api.Models.VarianteRegles.Classique, service.Creer("Alice").Partie.Variante);
            Assert.AreEqual(Kay.Api.Models.VarianteRegles.CouperLaTete,
                service.Creer("Alice", Kay.Api.Models.VarianteRegles.CouperLaTete).Partie.Variante);
        }

        [TestMethod]
        public void ModeLocal_PartieDemarreeEtUnJetonPourLesDeuxJoueurs()
        {
            var service = NouveauService();
            var acces = service.Creer("Alice", mode: ModePartie.Local, nomJoueur2: "Bob");
            Assert.AreEqual(EtatPartie.EnCours, acces.Partie.Etat);
            Assert.AreEqual(ModePartie.Local, acces.Partie.Mode);

            // Le même jeton joue pour le joueur actif, quel qu'il soit.
            int actif = acces.Partie.JoueurActifId;
            var apres = service.Jouer(acces.Partie.Code, acces.Jeton, actif == 1 ? 0 : 6);
            Assert.AreEqual(1, apres.Historique.Count(h => h.Contains("joue")));
            Assert.IsEmpty(service.PartiesEnAttente(), "Une partie locale n'est pas proposée au pairage.");
        }

        [TestMethod]
        public void ModeLocal_SansSecondNom_EstRefuse() =>
            Assert.ThrowsExactly<ActionRefuseeException>(() => NouveauService().Creer("Alice", mode: ModePartie.Local));

        [TestMethod]
        public void PartiesEnAttente_ListeLesPartiesEnLigneNonCompletes()
        {
            var service = NouveauService();
            var a = service.Creer("Alice", VarianteRegles.CouperLaTete);
            var b = service.Creer("Bob");
            service.Rejoindre(b.Partie.Code, "Chloé");

            var enAttente = service.PartiesEnAttente();
            Assert.HasCount(1, enAttente);
            Assert.AreEqual(a.Partie.Code, enAttente[0].Code);
            Assert.AreEqual("Alice", enAttente[0].NomCreateur);
            Assert.AreEqual(VarianteRegles.CouperLaTete, enAttente[0].Variante);
        }

        [TestMethod]
        public void Annuler_ParLeCreateur_RetireLaPartie()
        {
            var service = NouveauService();
            var a = service.Creer("Alice");
            service.Annuler(a.Partie.Code, a.Jeton);
            Assert.IsEmpty(service.PartiesEnAttente());
            Assert.ThrowsExactly<PartieIntrouvableException>(() => service.Lire(a.Partie.Code));
        }

        [TestMethod]
        public void Annuler_PartieCommencee_EstRefuse()
        {
            var service = NouveauService();
            var a = service.Creer("Alice");
            service.Rejoindre(a.Partie.Code, "Bob");
            Assert.ThrowsExactly<ActionRefuseeException>(() => service.Annuler(a.Partie.Code, a.Jeton));
        }

        [TestMethod]
        public void Rejoindre_RefuseTroisiemeJoueur()
        {
            var service = NouveauService();
            var p = service.Creer("Alice");
            service.Rejoindre(p.Partie.Code, "Bob");
            Assert.ThrowsExactly<ActionRefuseeException>(() => service.Rejoindre(p.Partie.Code, "Charlie"));
        }

        [TestMethod]
        public void Jouer_RefuseJetonIncorrect()
        {
            var service = NouveauService();
            var p = service.Creer("Alice");
            service.Rejoindre(p.Partie.Code, "Bob");
            Assert.ThrowsExactly<AccesRefuseException>(() => service.Jouer(p.Partie.Code, "faux-jeton", 0));
        }

        [TestMethod]
        public void Jouer_RefuseCoupHorsTour()
        {
            var service = NouveauService();
            var a = service.Creer("Alice");
            var b = service.Rejoindre(a.Partie.Code, "Bob");
            var (jetonAttente, position) = b.Partie.JoueurActifId == 1 ? (b.Jeton, 6) : (a.Jeton, 0);
            Assert.ThrowsExactly<CoupInvalideException>(() => service.Jouer(a.Partie.Code, jetonAttente, position));
        }

        [TestMethod]
        public void ContinuerEtRejouer_HorsDuMomentPrevu_SontRefuses()
        {
            var service = NouveauService();
            var a = service.Creer("Alice");
            service.Rejoindre(a.Partie.Code, "Bob");
            Assert.ThrowsExactly<ActionRefuseeException>(() => service.Continuer(a.Partie.Code, a.Jeton));
            Assert.ThrowsExactly<ActionRefuseeException>(() => service.Rejouer(a.Partie.Code, a.Jeton));
            Assert.ThrowsExactly<AccesRefuseException>(() => service.Continuer(a.Partie.Code, "faux"));
        }

        [TestMethod]
        public void Lire_PartieInconnue_Introuvable() =>
            Assert.ThrowsExactly<PartieIntrouvableException>(() => NouveauService().Lire("INCONNU"));

        [TestMethod]
        public void JetonValide_ReconnaitLesDeuxJoueursSeulement()
        {
            var service = NouveauService();
            var a = service.Creer("Alice");
            var b = service.Rejoindre(a.Partie.Code, "Bob");
            Assert.IsTrue(service.JetonValide(a.Partie.Code, a.Jeton));
            Assert.IsTrue(service.JetonValide(a.Partie.Code, b.Jeton));
            Assert.IsFalse(service.JetonValide(a.Partie.Code, "faux"));
            Assert.IsFalse(service.JetonValide("INCONNU", a.Jeton));
        }

        [TestMethod]
        public void Version_AugmenteAChaqueModification()
        {
            var service = NouveauService();
            var a = service.Creer("Alice");
            var b = service.Rejoindre(a.Partie.Code, "Bob");
            Assert.IsGreaterThan(a.Partie.Version, b.Partie.Version);
        }
    }
}
