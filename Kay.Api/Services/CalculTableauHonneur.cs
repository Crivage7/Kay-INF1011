using Kay.Api.DTOs;
using Kay.Api.Models;

namespace Kay.Api.Services
{
    // Calcul pur du tableau d'honneur à partir des résultats enregistrés.
    // Les joueurs n'ont pas de compte : un joueur est identifié par son nom, sans tenir compte de la casse.
   
    public static class CalculTableauHonneur
    {
        public const int TailleClassement = 20;
        public const int NombreMeilleursResultats = 10;

        public static TableauHonneurDto Calculer(IEnumerable<ResultatPartie> resultats)
        {
            var liste = resultats.ToList();
            return new TableauHonneurDto(Classement(liste), MeilleursResultats(liste));
        }

        private static List<LigneClassementDto> Classement(List<ResultatPartie> resultats) =>
            resultats
                .SelectMany(r => new[]
                {
                    (Nom: r.NomJoueur1, Issue: Issue(r, 1)),
                    (Nom: r.NomJoueur2, Issue: Issue(r, 2)),
                })
                .GroupBy(p => p.Nom.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new LigneClassementDto(
                    g.First().Nom.Trim(),
                    Victoires: g.Count(p => p.Issue > 0),
                    Nulles: g.Count(p => p.Issue == 0),
                    Defaites: g.Count(p => p.Issue < 0),
                    Parties: g.Count()))
                .OrderByDescending(l => l.Victoires)
                .ThenByDescending(l => l.Nulles)
                .ThenBy(l => l.Defaites)
                .ThenBy(l => l.Nom, StringComparer.CurrentCultureIgnoreCase)
                .Take(TailleClassement)
                .ToList();

        // Les victoires les plus larges (graines du vainqueur), puis les plus rapides (manches).
        private static List<MeilleurResultatDto> MeilleursResultats(List<ResultatPartie> resultats) =>
            resultats
                .Where(r => r.VainqueurId is not null)
                .Select(r => r.VainqueurId == 1
                    ? new MeilleurResultatDto(r.Id, r.NomJoueur1, r.NomJoueur2, r.GrainesJoueur1, r.Manches, r.Variante, r.TermineeLe)
                    : new MeilleurResultatDto(r.Id, r.NomJoueur2, r.NomJoueur1, r.GrainesJoueur2, r.Manches, r.Variante, r.TermineeLe))
                .OrderByDescending(m => m.GrainesVainqueur)
                .ThenBy(m => m.Manches)
                .ThenBy(m => m.TermineeLe)
                .Take(NombreMeilleursResultats)
                .ToList();

        // 1 = victoire, 0 = nulle, -1 = défaite, pour le joueur indiqué.
        private static int Issue(ResultatPartie r, int joueurId) =>
            r.VainqueurId is null ? 0 : r.VainqueurId == joueurId ? 1 : -1;
    }
}
