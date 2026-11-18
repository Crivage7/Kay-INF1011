using Kay.Api.DTOs;

namespace Kay.Api.Services
{
    // Point d'entrée applicatif du tableau d'honneur : lit les résultats et délègue le calcul.
    public sealed class TableauHonneurService(IDepotResultats depot)
    {
        public TableauHonneurDto Obtenir() => CalculTableauHonneur.Calculer(depot.Lister());
    }
}
