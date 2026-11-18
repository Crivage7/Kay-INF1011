namespace Kay.Api.Models
{
    // Résultat d'une partie terminée, conservé pour le tableau d'honneur.
  
    public class ResultatPartie
    {
        public int Id { get; init; }
        public string NomJoueur1 { get; init; } = string.Empty;
        public string NomJoueur2 { get; init; } = string.Empty;
        // 1 ou 2 ; null pour une partie nulle.
        public int? VainqueurId { get; init; }
        // Graines de chaque joueur en fin de partie (trous + réceptacle).
        public int GrainesJoueur1 { get; init; }
        public int GrainesJoueur2 { get; init; }
        public int Manches { get; init; }
        public int CoupsJoueur1 { get; init; }
        public int CoupsJoueur2 { get; init; }
        public VarianteRegles Variante { get; init; }
        public ModePartie Mode { get; init; }
        public DateTime TermineeLe { get; init; }

        public static ResultatPartie Depuis(Partie partie, DateTime termineeLe)
        {
            if (!partie.EstTerminee) throw new InvalidOperationException("La partie n'est pas terminée.");
            int Graines(int rangee) => partie.Plateau.CompterGraines(rangee) + partie.Plateau.ObtenirReceptacle(rangee).NombreGraines;
            return new ResultatPartie
            {
                NomJoueur1 = partie.Joueur1.Nom,
                NomJoueur2 = partie.Joueur2!.Nom,
                VainqueurId = partie.VainqueurId,
                GrainesJoueur1 = Graines(partie.Joueur1.NumeroRangee),
                GrainesJoueur2 = Graines(partie.Joueur2.NumeroRangee),
                Manches = partie.NumeroManche,
                CoupsJoueur1 = partie.CoupsJoues[0],
                CoupsJoueur2 = partie.CoupsJoues[1],
                Variante = partie.Variante,
                Mode = partie.Mode,
                TermineeLe = termineeLe,
            };
        }
    }
}
