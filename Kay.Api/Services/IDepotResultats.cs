using Kay.Api.Models;

namespace Kay.Api.Services
{
    
    /// Stockage des résultats de parties.
    
    public interface IDepotResultats
    {
        void Enregistrer(ResultatPartie resultat);

        IReadOnlyList<ResultatPartie> Lister();
    }
}
