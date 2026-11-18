using Kay.Api.Models;

namespace Kay.Api.DTOs
{
   
    // État d'une partie tel qu'exposé au client. Séparé du modèle pour ne pas
    // coupler le contrat réseau à la structure interne du domaine.
   
    public record PartieDto(
        string Code,
        ModePartie Mode,
        VarianteRegles Variante,
        EtatPartie Etat,
        int NumeroManche,
        int JoueurActifId,
        int? VainqueurId,
        bool EstNulle,
        int? JoueurPouvantDeclarerNulle,
        long Version,
        IReadOnlyList<JoueurDto> Joueurs,
        IReadOnlyList<CaseDto> Cases,
        IReadOnlyList<int> Receptacles,
        IReadOnlyList<string> Historique,
        BilanMancheDto? DernierBilan,
        IReadOnlyList<int> Confirmations,
        bool DeuxLotsPermis,
        IReadOnlyList<int> CoupsJoues,
        IReadOnlyList<int> GrainesCapturees,
        LotConserveDto? LotConserve)
    {
        public static PartieDto Depuis(Partie partie, long version)
        {
            var joueurs = new List<JoueurDto> { JoueurDto.Depuis(partie.Joueur1) };
            if (partie.Joueur2 is not null) joueurs.Add(JoueurDto.Depuis(partie.Joueur2));
            return new PartieDto(
                partie.Code,
                partie.Mode,
                partie.Variante,
                partie.Etat,
                partie.NumeroManche,
                partie.JoueurActifId,
                partie.VainqueurId,
                partie.EstNulle,
                partie.JoueurPouvantDeclarerNulle,
                version,
                joueurs,
                partie.Plateau.Cases.Select(c => new CaseDto(c.Position, c.NombreGraines, c.EstActive)).ToList(),
                partie.Plateau.Receptacles.Select(r => r.NombreGraines).ToList(),
                partie.Historique.ToList(),
                partie.DernierBilan is null ? null : BilanMancheDto.Depuis(partie.DernierBilan),
                partie.Confirmations.Order().ToList(),
                partie.DeuxLotsPermis,
                partie.CoupsJoues.ToList(),
                partie.GrainesCapturees.ToList(),
                LotConserveDto.Depuis(partie));
        }
    }

    // Second lot du premier tour gardé en réserve par un joueur (2.1), ou null.
    public record LotConserveDto(int JoueurId, int Position, int Graines)
    {
        public static LotConserveDto? Depuis(Partie partie)
        {
            Joueur? joueur = new[] { partie.Joueur1, partie.Joueur2 }
                .FirstOrDefault(j => j is not null && partie.Plateau.ObtenirLotConserve(j.NumeroRangee) is not null);
            if (joueur is null) return null;
            LotConserve lot = partie.Plateau.ObtenirLotConserve(joueur.NumeroRangee)!;
            return new LotConserveDto(joueur.Id, lot.Position, lot.Graines);
        }
    }

    public record JoueurDto(int Id, string Nom, int NumeroRangee)
    {
        public static JoueurDto Depuis(Joueur joueur) => new(joueur.Id, joueur.Nom, joueur.NumeroRangee);
    }

    public record CaseDto(int Position, int NombreGraines, bool EstActive);

    // Bilan de la dernière manche ; listes indexées par rangée (0 = joueur 1, 1 = joueur 2)
    public record BilanMancheDto(
        int NumeroManche,
        IReadOnlyList<int> GrainesRamassees,
        IReadOnlyList<int> Stocks,
        IReadOnlyList<int> TrousActifs,
        IReadOnlyList<int> Reserves)
    {
        public static BilanMancheDto Depuis(BilanManche bilan) =>
            new(bilan.NumeroManche, bilan.GrainesRamassees, bilan.Stocks, bilan.TrousActifs, bilan.Reserves);
    }

    // Partie proposée sur l'écran de pairage.
    public record PartieEnAttenteDto(string Code, string NomCreateur, VarianteRegles Variante, DateTime CreeeLe)
    {
        public static PartieEnAttenteDto Depuis(Partie partie, DateTime creeeLe) =>
            new(partie.Code, partie.Joueur1.Nom, partie.Variante, creeeLe);
    }

    // Création d'une partie ; <c>NomJoueur2</c> est requis en mode local (même écran).
    public record CreerPartieRequete(string Nom, VarianteRegles Variante = VarianteRegles.Classique,
        ModePartie Mode = ModePartie.EnLigne, string? NomJoueur2 = null);

    public record RejoindrePartieRequete(string Nom);

    // Coup joué ; <c>PositionConservee</c> désigne le second lot, permis au premier tour seulement.
    public record JouerRequete(int Position, string Jeton, int? PositionConservee = null);

    public record JetonRequete(string Jeton);

    public record AccesPartieReponse(
        PartieDto Partie,
        int JoueurId,
        string Jeton
    );
}
