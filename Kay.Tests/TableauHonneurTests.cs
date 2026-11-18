using Kay.Api.Models;
using Kay.Api.Persistance;
using Kay.Api.Services;
using Kay.Tests.Outils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kay.Tests
{
    // Tableau d'honneur : calcul du classement, persistance SQLite, enregistrement des fins de partie.
    [TestClass]
    public sealed class TableauHonneurTests
    {
        private static ResultatPartie Resultat(string j1, string j2, int? vainqueur, int graines1 = 24, int graines2 = 24, int manches = 3) => new()
        {
            NomJoueur1 = j1, NomJoueur2 = j2, VainqueurId = vainqueur,
            GrainesJoueur1 = graines1, GrainesJoueur2 = graines2, Manches = manches,
            Variante = VarianteRegles.Classique, Mode = ModePartie.EnLigne, TermineeLe = new DateTime(2026, 11, 7, 12, 0, 0, DateTimeKind.Utc),
        };

        [TestMethod]
        public void Classement_ParVictoiresPuisNulles()
        {
            var tableau = CalculTableauHonneur.Calculer([
                Resultat("Alice", "Bob", 1),
                Resultat("Alice", "Chloé", 1),
                Resultat("Bob", "Chloé", 2),
                Resultat("Bob", "Chloé", null),
            ]);

            CollectionAssert.AreEqual(new[] { "Alice", "Chloé", "Bob" }, tableau.Classement.Select(l => l.Nom).ToArray());
            var chloe = tableau.Classement[1];
            Assert.AreEqual((1, 1, 1, 3), (chloe.Victoires, chloe.Nulles, chloe.Defaites, chloe.Parties));
        }

        [TestMethod]
        public void Classement_UnMemeNomQuelleQueSoitLaCasse()
        {
            var tableau = CalculTableauHonneur.Calculer([Resultat("Alice", "Bob", 1), Resultat("alice", "Bob", 1)]);
            Assert.AreEqual(2, tableau.Classement.Single(l => l.Nom == "Alice").Victoires);
        }

        [TestMethod]
        public void MeilleursResultats_VictoiresLesPlusLargesDAbord()
        {
            var tableau = CalculTableauHonneur.Calculer([
                Resultat("Alice", "Bob", 1, graines1: 32, graines2: 16),
                Resultat("Alice", "Bob", 2, graines1: 4, graines2: 44, manches: 9),
                Resultat("Alice", "Bob", null),
            ]);

            Assert.HasCount(2, tableau.MeilleursResultats);
            var premier = tableau.MeilleursResultats[0];
            Assert.AreEqual(("Bob", "Alice", 44, 9), (premier.Vainqueur, premier.Perdant, premier.GrainesVainqueur, premier.Manches));
        }

        [TestMethod]
        public void TableauVide_SansResultat()
        {
            var tableau = CalculTableauHonneur.Calculer([]);
            Assert.IsEmpty(tableau.Classement);
            Assert.IsEmpty(tableau.MeilleursResultats);
        }

        [TestMethod]
        public void DepotSqlite_LesResultatsSurviventAUnRedemarrage()
        {
            string fichier = Path.Combine(Path.GetTempPath(), $"kay-test-{Guid.NewGuid():N}.db");
            try
            {
                // Premier « démarrage » : création du schéma et enregistrement.
                new DepotResultatsEf(Fabrique(fichier), NullLogger<DepotResultatsEf>.Instance).Enregistrer(Resultat("Alice", "Bob", 1));

                // Second « démarrage » : nouvelle fabrique, même fichier.
                var resultats = new DepotResultatsEf(Fabrique(fichier), NullLogger<DepotResultatsEf>.Instance).Lister();
                Assert.HasCount(1, resultats);
                Assert.AreEqual("Alice", resultats[0].NomJoueur1);
                Assert.AreEqual(1, resultats[0].VainqueurId);
                Assert.AreEqual(VarianteRegles.Classique, resultats[0].Variante);
                Assert.AreEqual(DateTimeKind.Utc, resultats[0].TermineeLe.Kind, "La date est relue en UTC.");
                Assert.AreEqual(new DateTime(2026, 11, 7, 12, 0, 0, DateTimeKind.Utc), resultats[0].TermineeLe);
            }
            finally
            {
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                File.Delete(fichier);
            }
        }

        [TestMethod]
        public void DepotSqlite_ErreurInattendue_NInterromptPasLaPartie()
        {
            // Une erreur autre que SQL (ici levée par la fabrique de contextes) est journalisée, jamais propagée.
            var depot = new DepotResultatsEf(new FabriqueEnPanne(), NullLogger<DepotResultatsEf>.Instance);
            depot.Enregistrer(Resultat("Alice", "Bob", 1));
        }

        [TestMethod]
        public void Service_EnregistreChaqueFinDePartieUneSeuleFois()
        {
            var depot = new DepotResultatsMemoire();
            var service = new PartieService([new ReglesKay()], depot);
            var acces = service.Creer("Alice", mode: ModePartie.Local, nomJoueur2: "Bob");

            JouerJusquALaFin(service, acces.Partie.Code, acces.Jeton);
            Assert.HasCount(1, depot.Resultats);
            service.Lire(acces.Partie.Code);
            Assert.HasCount(1, depot.Resultats, "Relire l'état n'enregistre pas une seconde fois.");

            // Nouvelle partie avec les mêmes joueurs : un second résultat.
            service.Rejouer(acces.Partie.Code, acces.Jeton);
            JouerJusquALaFin(service, acces.Partie.Code, acces.Jeton);
            Assert.HasCount(2, depot.Resultats);

            var resultat = depot.Resultats[0];
            Assert.AreEqual(("Alice", "Bob"), (resultat.NomJoueur1, resultat.NomJoueur2));
            Assert.AreEqual(48, resultat.GrainesJoueur1 + resultat.GrainesJoueur2);
            Assert.AreEqual(ModePartie.Local, resultat.Mode);
        }

        private static IDbContextFactory<KayDbContext> Fabrique(string fichier)
        {
            var options = new DbContextOptionsBuilder<KayDbContext>().UseSqlite($"Data Source={fichier}").Options;
            var fabrique = new FabriqueContexte(options);
            using var contexte = fabrique.CreateDbContext();
            contexte.Database.EnsureCreated();
            return fabrique;
        }

        private sealed class FabriqueContexte(DbContextOptions<KayDbContext> options) : IDbContextFactory<KayDbContext>
        {
            public KayDbContext CreateDbContext() => new(options);
        }

        private sealed class FabriqueEnPanne : IDbContextFactory<KayDbContext>
        {
            public KayDbContext CreateDbContext() => throw new InvalidOperationException("Base indisponible.");
        }

        // Joue des coups aléatoires (en mode local, le jeton joue pour les deux) jusqu'à la fin.
        private static void JouerJusquALaFin(PartieService service, string code, string jeton)
        {
            var hasard = new Random(7);
            var partie = service.Lire(code);
            while (partie.Etat != EtatPartie.Terminee)
            {
                if (partie.Etat == EtatPartie.FinDeManche) { partie = service.Continuer(code, jeton); continue; }
                var choix = partie.Cases.Where(c => c.Position / 6 == partie.JoueurActifId - 1 && c.EstActive && c.NombreGraines > 0).ToList();
                try { partie = service.Jouer(code, jeton, choix[hasard.Next(choix.Count)].Position); }
                catch (Kay.Api.Exceptions.CoupInvalideException) { /* relais sans fin : un autre trou */ }
            }
        }
    }
}
