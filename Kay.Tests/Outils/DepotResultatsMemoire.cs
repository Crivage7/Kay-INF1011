using Kay.Api.Models;
using Kay.Api.Services;

namespace Kay.Tests.Outils
{
    // Dépôt en mémoire pour tester le service sans base de données.
    internal sealed class DepotResultatsMemoire : IDepotResultats
    {
        public List<ResultatPartie> Resultats { get; } = new();
        public void Enregistrer(ResultatPartie resultat) => Resultats.Add(resultat);
        public IReadOnlyList<ResultatPartie> Lister() => Resultats;
    }
}
