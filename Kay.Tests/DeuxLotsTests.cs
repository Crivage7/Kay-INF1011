using Kay.Api.DTOs;
using Kay.Api.Exceptions;
using Kay.Api.Models;
using Kay.Api.Services;
using static Kay.Tests.Outils.Fabrique;

namespace Kay.Tests
{
    // Premier tour de la partie : un ou deux lots 
    [TestClass]
    public sealed class DeuxLotsTests
    {
        private static readonly Joueur J1 = new(1, "A", 0);

        private static (Plateau Plateau, ResultatDistribution Resultat) DeuxLots(int[] cases, int position, int conservee)
        {
            var plateau = Plateau.Depuis(cases);
            return (plateau, new ReglesKay().DistribuerDeuxLots(plateau, J1, position, conservee));
        }

        [TestMethod]
        public void Mort_LeLotConserveEstDistribueDepuisSonTrouDOrigine()
        {
            // Lot 1 (trou 0, 1 graine) : 1 → mort. 
            var (plateau, resultat) = DeuxLots([1, 0, 0, 2, 0, 0, 2, 2, 2, 2, 2, 2], 0, 3);
            CollectionAssert.AreEqual(new[] { 0, 1, 0, 0, 1, 1, 2, 2, 2, 2, 2, 2 }, Graines(plateau));
            Assert.AreEqual(IssueDistribution.Mort, resultat.Issue);
        }

        [TestMethod]
        public void Capture_LeJoueurRejoueEtGardeSonLotEnReserve()
        {
            // Lot 1 : 1 (3→4) capture 4 
            var partie = PartieEnCours([1, 3, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2]);
            partie.JouerCase(1, 0, positionConservee: 3);
            CollectionAssert.AreEqual(new[] { 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 2 }, Graines(partie));
            CollectionAssert.AreEqual(new[] { 4, 0 }, Receptacles(partie));
            Assert.AreEqual(new LotConserve(3, 2), partie.Plateau.ObtenirLotConserve(0));
            Assert.AreEqual(1, partie.JoueurActifId, "Après une capture, le joueur garde son tour.");
            Assert.AreEqual(20, partie.Plateau.CompterGraines(), "Les graines en main comptent dans le total.");
            StringAssert.Contains(partie.Historique[^1], "il rejoue");
        }

        [TestMethod]
        public void MortAuCoupSuivant_LeLotConserveEstSemeDepuisSonTrou()
        {
            // Après la capture du lot 1, J1 joue le trou 2 (2 graines) : 3, 4 → le trou 4 était vide, il est mort.
            // Il sème alors son lot conservé (2 graines) à partir du trou 3 : 4, 5 → mort, le tour passe.
            var partie = PartieEnCours([1, 3, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2]);
            partie.JouerCase(1, 0, positionConservee: 3);
            Jouer(partie, 2);
            CollectionAssert.AreEqual(new[] { 0, 0, 0, 1, 2, 1, 2, 2, 2, 2, 2, 2 }, Graines(partie));
            Assert.IsNull(partie.Plateau.ObtenirLotConserve(0));
            Assert.AreEqual(2, partie.JoueurActifId);
            StringAssert.Contains(partie.Historique[^1], "sème son lot conservé");
        }

        [TestMethod]
        public void LotConserve_ResteEnReservePendantPlusieursCaptures_JusquALaPremiereMort()
        {
            // Lot 1 : 1 (3→4) capture ; lot conservé = trou 5 (2 graines).
            var partie = PartieEnCours([1, 3, 1, 3, 1, 2, 2, 2, 2, 2, 2, 2]);
            partie.JouerCase(1, 0, positionConservee: 5);
            // Trou 2 : 3 (3→4), nouvelle capture ; le lot est toujours en réserve.
            Jouer(partie, 2);
            Assert.AreEqual(1, partie.JoueurActifId);
            Assert.AreEqual(new LotConserve(5, 2), partie.Plateau.ObtenirLotConserve(0));
            CollectionAssert.AreEqual(new[] { 8, 0 }, Receptacles(partie));

            // Trou 4 : 5 (vide) → mort. Le lot part du trou 5 : 6, 7 (2→3, relais) 8, 9, 10 (2→3, relais) 11, 0, 1 → mort.
            Jouer(partie, 4);
            CollectionAssert.AreEqual(new[] { 1, 1, 0, 0, 0, 1, 3, 0, 3, 3, 0, 3 }, Graines(partie));
            Assert.IsNull(partie.Plateau.ObtenirLotConserve(0));
            Assert.AreEqual(2, partie.JoueurActifId);
            Assert.AreEqual(23, partie.Plateau.CompterGraines());
        }

        [TestMethod]
        [DataRow(4, 0)]
        [DataRow(5, 1)]
        public void PlateauInitial_CaptureAuPremierLot_LeJoueurGardeLaMainEtSonLot(int position, int conservee)
        {
            // Combinaisons du plateau initial où le premier lot capture
            var partie = PartieEnCours([4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4]);
            partie.JouerCase(1, position, conservee);
            Assert.AreEqual(1, partie.JoueurActifId);
            Assert.AreEqual(new LotConserve(conservee, 4), partie.Plateau.ObtenirLotConserve(0));
            Assert.AreEqual(4, partie.GrainesCapturees[0]);
            Assert.AreEqual(48, partie.Plateau.CompterGraines());
            Assert.IsFalse(partie.DeuxLotsPermis, "Prendre deux lots reste réservé au premier coup.");
        }

        [TestMethod]
        public void Dto_ExposeLeLotConserve()
        {
            var partie = PartieEnCours([4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4]);
            Assert.IsNull(PartieDto.Depuis(partie, 1).LotConserve);
            partie.JouerCase(1, 4, 0);
            Assert.AreEqual(new LotConserveDto(1, 0, 4), PartieDto.Depuis(partie, 1).LotConserve);
        }

        [TestMethod]
        public void LeSecondTrouEstVideDesLeChoix()
        {
            // Le trou 3 est vidé au départ ; le lot 1 (3 graines) y tombe en dernier et le joueur est mort.
            // Lot conservé (4 graines depuis 3) : 4, 5, 6, 7.
            var (plateau, _) = DeuxLots([3, 0, 0, 4, 0, 0, 0, 0, 2, 2, 2, 2], 0, 3);
            CollectionAssert.AreEqual(new[] { 0, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2 }, Graines(plateau));
        }

        [TestMethod]
        public void FinDeMancheAvantLeSecondLot_LotAuReceptacleDuJoueur()
        {
            // Les deux trous de J1 sont vidés ; le lot 1 sort de sa rangée, qui est alors vide.
            // La fin de manche est prioritaire, les 2 graines du lot conservé vont au réceptacle de J1.
            var (plateau, _) = DeuxLots([2, 0, 0, 0, 0, 1, 0, 2, 2, 2, 2, 2], 5, 0);
            CollectionAssert.AreEqual(new[] { 0, 0, 0, 0, 0, 0, 1, 2, 2, 2, 2, 2 }, Graines(plateau));
            Assert.AreEqual(2, plateau.ObtenirReceptacle(0).NombreGraines);
            Assert.AreEqual(13, plateau.CompterGraines());
        }

        [TestMethod]
        public void PremierTour_DeuxLotsSurLePlateauInitial()
        {
            var partie = PartieEnCours([4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4]);
            Assert.IsTrue(partie.DeuxLotsPermis);
            partie.JouerCase(1, 0, positionConservee: 3);
            Assert.IsFalse(partie.DeuxLotsPermis);
            Assert.AreEqual(48, partie.Plateau.CompterGraines());
            StringAssert.Contains(partie.Historique[^1], "en gardant le trou 3 en réserve");
        }

        [TestMethod]
        public void ToursSuivants_DeuxLotsRefuses()
        {
            var partie = PartieEnCours([1, 0, 0, 0, 0, 3, 2, 2, 2, 2, 2, 2]);
            Jouer(partie, 0);
            Assert.IsFalse(partie.DeuxLotsPermis);
            var erreur = Assert.ThrowsExactly<CoupInvalideException>(() => partie.JouerCase(2, 6, positionConservee: 7));
            StringAssert.Contains(erreur.Message, "premier tour");
        }

        [TestMethod]
        [DataRow(0, DisplayName = "même trou")]
        [DataRow(6, DisplayName = "rangée adverse")]
        [DataRow(1, DisplayName = "trou vide")]
        public void SecondLotInvalide_EstRefuseSansModifierLaPartie(int conservee)
        {
            int[] depart = [4, 0, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4];
            var partie = PartieEnCours(depart);
            Assert.ThrowsExactly<CoupInvalideException>(() => partie.JouerCase(1, 0, conservee));
            CollectionAssert.AreEqual(depart, Graines(partie));
            Assert.IsTrue(partie.DeuxLotsPermis);
        }

        [TestMethod]
        public void NouvellePartie_LesDeuxLotsRedeviennentPermis()
        {
            var partie = PartieEnCours([0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1], receptacleJ1: 8, receptacleJ2: 30);
            Jouer(partie, 5);
            partie.DemanderNouvellePartie(1);
            partie.DemanderNouvellePartie(2);
            Assert.IsTrue(partie.DeuxLotsPermis);
        }
    }
}
