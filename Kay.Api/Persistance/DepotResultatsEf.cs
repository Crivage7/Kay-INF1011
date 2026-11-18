using Kay.Api.Models;
using Kay.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Kay.Api.Persistance
{
    public sealed class DepotResultatsEf(IDbContextFactory<KayDbContext> fabrique, ILogger<DepotResultatsEf> journal) : IDepotResultats
    {
        public void Enregistrer(ResultatPartie resultat)
        {
            try
            {
                using KayDbContext contexte = fabrique.CreateDbContext();
                contexte.Resultats.Add(resultat);
                contexte.SaveChanges();
            }
            catch (Exception e)
            {
                journal.LogError(e, "Impossible d'enregistrer le résultat de la partie {Joueur1} contre {Joueur2}",
                    resultat.NomJoueur1, resultat.NomJoueur2);
            }
        }

        public IReadOnlyList<ResultatPartie> Lister()
        {
            using KayDbContext contexte = fabrique.CreateDbContext();
            return contexte.Resultats.AsNoTracking().ToList();
        }
    }
}
