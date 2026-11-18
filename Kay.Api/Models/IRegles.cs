namespace Kay.Api.Models
{
    /*
    Patron Strategy : contrat d'une variante de règles. "Partie" ne dépend que de cette
    abstraction (inversion des dépendances) ; une nouvelle variante s'ajoute sans modifier Partie.
    Les méthodes reprennent celles de ReglesKay dans le diagramme de classes ; Redistribuer et
    PeutJouer s'y ajoutent pour que Partie n'ait aucune règle à connaître.
    */
    public interface IRegles
    {
        VarianteRegles Variante { get; }

        // Indique si le joueur peut choisir ce trou, avec le motif d'un éventuel refus.
        Verification ChoixAutorise(Plateau plateau, Joueur joueur, int position);

        /*
         Distribue les graines du trou choisi, enchaîne les relais et les captures ; si le joueur
         est « mort » et garde un lot conservé, ce lot est semé à son tour (2.2).
         Modifie le plateau reçu ; lève "Exceptions.CoupInvalideException" si le coup
         ne se termine jamais (le plateau reçu est alors à jeter).
        */
        
        ResultatDistribution DistribuerEtCapturer(Plateau plateau, Joueur joueur, int position);

        /*
         Premier tour de la partie avec deux lots (2.1) : met le second lot en réserve et distribue
         le premier ; le lot conservé n'est semé que si le joueur est « mort ».
         Mêmes garanties que "DistribuerEtCapturer".
        */
        ResultatDistribution DistribuerDeuxLots(Plateau plateau, Joueur joueur, int position, int positionConservee);

        // Vrai si le joueur a au moins un coup autorisé qui se termine.
        bool PeutJouer(Plateau plateau, Joueur joueur);

        bool MancheTerminee(Plateau plateau);

        // Ramasse les graines restantes et remplit les trous pour la manche suivante.
        BilanManche Redistribuer(Plateau plateau, int numeroManche);

        // Rangée gagnante après redistribution, ou null si la partie continue.
        int? DeterminerVainqueur(Plateau plateau);

        /*
         Rangée du joueur qui peut déclarer la partie nulle après ce bilan, ou null.
         <paramref name="stocksDebutManche"/> : graines de chaque joueur (trous + réceptacle) au début de la manche.
        */
        int? RangeePouvantDeclarerNulle(IReadOnlyList<int> stocksDebutManche, BilanManche bilan);
    }

    public sealed record Verification(bool EstAutorise, string? Motif)
    {
        public static readonly Verification Autorise = new(true, null);
        public static Verification Refuse(string motif) => new(false, motif);
    }

    public enum IssueDistribution
    {
        // La dernière graine a complété un trou à 4 : le joueur rejoue.
        Capture,
        // La dernière graine est tombée dans un trou vide : le tour passe.
        Mort
    }

    // Issue d'un coup ; LotConserveJoue indique que le lot conservé a été semé après une « mort ».
    public sealed record ResultatDistribution(IssueDistribution Issue, int GrainesCapturees, bool LotConserveJoue = false);

    // Résumé d'une fin de manche ; chaque liste est indexée par rangée (0 = J1, 1 = J2).
    public sealed record BilanManche(
        int NumeroManche,
        IReadOnlyList<int> GrainesRamassees,
        IReadOnlyList<int> Stocks,
        IReadOnlyList<int> TrousActifs,
        IReadOnlyList<int> Reserves);
}
