using Kay.Api.Models;
using static Kay.Tests.Outils.Fabrique;

namespace Kay.Tests
{
    [TestClass]
    public sealed class PlateauTests
    {
        [TestMethod]
        public void Plateau_InitialiseQuaranteHuitGraines() =>
            Assert.AreEqual(48, new Plateau().CompterGraines());

        [TestMethod]
        public void Plateau_InitialiseDouzeTrousActifsDeQuatreGraines() =>
            Assert.IsTrue(new Plateau().Cases.All(c => c.EstActive && c.NombreGraines == 4));

        [TestMethod]
        public void CompterGrainesEtDesactivees_ParRangee()
        {
            var plateau = Plateau.Depuis([X, 1, 2, 3, X, 0, 4, 4, 4, 4, 4, 4], receptacle1: 5);
            Assert.AreEqual(6, plateau.CompterGraines(0));
            Assert.AreEqual(24, plateau.CompterGraines(1));
            Assert.AreEqual(2, plateau.CompterDesactivees(0));
            Assert.AreEqual(0, plateau.CompterDesactivees(1));
            Assert.AreEqual(35, plateau.CompterGraines());
        }

        [TestMethod]
        public void Cloner_ProduitUneCopieIndependante()
        {
            var original = Plateau.Depuis([X, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11], 3, 4);
            var copie = original.Cloner();
            CollectionAssert.AreEqual(Graines(original), Graines(copie));
            copie.ObtenirCase(1).RetirerToutesLesGraines();
            copie.ObtenirReceptacle(0).AjouterGraines(1);
            Assert.AreEqual(1, original.ObtenirCase(1).NombreGraines);
            Assert.AreEqual(3, original.ObtenirReceptacle(0).NombreGraines);
        }

        [TestMethod]
        public void LotConserve_CompteDansLeTotal_EstCopieEtRejointLeReceptacle()
        {
            var plateau = new Plateau();
            plateau.ConserverLot(0, 2, plateau.ObtenirCase(2).RetirerToutesLesGraines());
            Assert.AreEqual(48, plateau.CompterGraines());

            var copie = plateau.Cloner();
            Assert.AreEqual(new LotConserve(2, 4), copie.ObtenirLotConserve(0));

            copie.RangerLotConserve(0);
            Assert.IsNull(copie.ObtenirLotConserve(0));
            Assert.AreEqual(4, copie.ObtenirReceptacle(0).NombreGraines);
            Assert.AreEqual(48, copie.CompterGraines());
            Assert.AreEqual(new LotConserve(2, 4), plateau.ObtenirLotConserve(0), "La copie est indépendante.");
        }

        [TestMethod]
        public void Desactiver_TrouVide()
        {
            var c = new Case(5, 0); c.Desactiver();
            Assert.AreEqual(0, c.NombreGraines);
            Assert.IsFalse(c.EstActive);
        }

        [TestMethod]
        public void Desactiver_TrouAvecGraines_EstRefuse() =>
            Assert.ThrowsExactly<InvalidOperationException>(() => new Case(5, 4).Desactiver());

        [TestMethod]
        public void Receptacle_RefuseQuantiteNegative() =>
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Receptacle().AjouterGraines(-1));
    }
}
