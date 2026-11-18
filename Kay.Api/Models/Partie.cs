using Kay.Api.Exceptions;

namespace Kay.Api.Models
{
    /*
     Contrôleur (GRASP) du déroulement d'une partie : il reçoit les actions des joueurs, vérifie
     qu'elles sont permises dans l'état courant et délègue toute décision de règle à <see cref="IRegles"/>.
    */
    public class Partie
    {
        private readonly IRegles _regles;
        private readonly Random _hasard;
        private readonly List<string> _historique = new();
        // Joueurs ayant confirmé le bilan de manche (ou demandé une nouvelle partie).
        private readonly HashSet<int> _confirmations = new();
        private int _premierJoueurDeLaManche;
        // Graines de chaque rangée (trous + réceptacle) au début de la manche, pour la règle de la nulle.
        private int[] _stocksDebutManche = new int[2];
        // Statistiques de la partie, indexées par joueur (0 = joueur 1).
        private readonly int[] _coupsJoues = new int[2];
        private readonly int[] _grainesCapturees = new int[2];

        public string Code { get; }
        public ModePartie Mode { get; }
        public VarianteRegles Variante => _regles.Variante;
        public Plateau Plateau { get; private set; }
        public Joueur Joueur1 { get; }
        public Joueur? Joueur2 { get; private set; }
        public int NumeroManche { get; private set; } = 1;
        public int JoueurActifId { get; private set; }
        public EtatPartie Etat { get; private set; } = EtatPartie.EnAttente;
        public bool EstTerminee => Etat == EtatPartie.Terminee;
        public int? VainqueurId { get; private set; }
        public bool EstNulle { get; private set; }
        // Joueur qui peut déclarer la nulle au bilan courant (2.5), ou null.
        public int? JoueurPouvantDeclarerNulle { get; private set; }
        public BilanManche? DernierBilan { get; private set; }
        // Vrai jusqu'au premier coup de la partie : le joueur peut alors prendre deux lots (2.1).
        public bool DeuxLotsPermis { get; private set; }
        public IReadOnlyList<string> Historique => _historique;
        public IReadOnlyCollection<int> Confirmations => _confirmations;
        public IReadOnlyList<int> CoupsJoues => _coupsJoues;
        public IReadOnlyList<int> GrainesCapturees => _grainesCapturees;

        public Partie(string code, string nomCreateur, IRegles regles, ModePartie mode = ModePartie.EnLigne, Random? hasard = null)
            : this(code, nomCreateur, regles, mode, new Plateau(), hasard) { }

        // Partie sur un plateau dans un état donné (tests).
        internal Partie(string code, string nomCreateur, IRegles regles, ModePartie mode, Plateau plateau, Random? hasard = null)
        {
            Code = code;
            Mode = mode;
            _regles = regles;
            _hasard = hasard ?? Random.Shared;
            Plateau = plateau;
            Joueur1 = new Joueur(1, nomCreateur, 0);
        }

        public Joueur ObtenirJoueur(int id) => id switch
        {
            1 => Joueur1,
            2 => Joueur2 ?? throw new ActionRefuseeException("Le joueur 2 n'a pas encore rejoint la partie."),
            _ => throw new ArgumentOutOfRangeException(nameof(id))
        };

        public void AjouterAdversaire(string nom)
        {
            if (Joueur2 is not null || Etat != EtatPartie.EnAttente)
                throw new ActionRefuseeException("Cette partie est déjà complète.");
            Joueur2 = new Joueur(2, nom, 1);
        }

        // Démarre la partie ; le premier joueur est désigné aléatoirement (2.1).
        public void Demarrer() => Demarrer(_hasard.Next(1, 3));

        public void Demarrer(int premierJoueurId)
        {
            if (Etat != EtatPartie.EnAttente) throw new ActionRefuseeException("La partie a déjà commencé.");
            if (Joueur2 is null) throw new ActionRefuseeException("Il faut deux joueurs pour commencer.");
            Etat = EtatPartie.EnCours;
            DeuxLotsPermis = true;
            _premierJoueurDeLaManche = premierJoueurId;
            _historique.Add($"Partie commencée. {ObtenirJoueur(premierJoueurId).Nom} débute.");
            MemoriserStocksDebutManche();
            DonnerLaMain(premierJoueurId);
        }

        /*
         Joue le trou choisi ; au premier tour de la partie, un second trou peut être choisi comme
         lot conservé (2.1). Le coup est simulé sur une copie du plateau puis adopté d'un bloc :
         un coup refusé ne modifie jamais l'état de la partie.
        */
        public void JouerCase(int joueurId, int position, int? positionConservee = null)
        {
            if (Etat != EtatPartie.EnCours) throw new ActionRefuseeException("Aucune manche n'est en cours.");
            if (joueurId != JoueurActifId) throw new CoupInvalideException("Ce n'est pas votre tour.");
            Joueur joueur = ObtenirJoueur(joueurId);
            Verification verification = _regles.ChoixAutorise(Plateau, joueur, position);
            if (!verification.EstAutorise) throw new CoupInvalideException(verification.Motif!);
            if (positionConservee is int conservee) VerifierLotConserve(joueur, position, conservee);

            Plateau essai = Plateau.Cloner();
            ResultatDistribution resultat = positionConservee is int lot
                ? _regles.DistribuerDeuxLots(essai, joueur, position, lot)
                : _regles.DistribuerEtCapturer(essai, joueur, position);
            Plateau = essai;
            DeuxLotsPermis = false;
            _coupsJoues[joueurId - 1]++;
            _grainesCapturees[joueurId - 1] += resultat.GrainesCapturees;

            string reserve = positionConservee is int second ? $" en gardant le trou {second} en réserve" : "";
            string lotJoue = resultat.LotConserveJoue ? ", est mort et sème son lot conservé" : "";
            string captures = resultat.GrainesCapturees > 0 ? $" et capture {resultat.GrainesCapturees} graines" : "";
            string suite = resultat.Issue == IssueDistribution.Capture ? " : il rejoue." : ", tour terminé.";
            _historique.Add($"{joueur.Nom} joue le trou {position}{reserve}{lotJoue}{captures}{suite}");

            // La fin de manche est prioritaire sur le droit de rejouer après une capture (2.3).
            if (_regles.MancheTerminee(Plateau)) { TerminerManche(); return; }
            DonnerLaMain(resultat.Issue == IssueDistribution.Capture ? joueurId : AutreJoueur(joueurId));
        }

        private void VerifierLotConserve(Joueur joueur, int position, int conservee)
        {
            if (!DeuxLotsPermis) throw new CoupInvalideException("Prendre deux lots n'est permis qu'au premier tour de la partie.");
            if (conservee == position) throw new CoupInvalideException("Choisissez deux trous différents pour les deux lots.");
            Verification verification = _regles.ChoixAutorise(Plateau, joueur, conservee);
            if (!verification.EstAutorise) throw new CoupInvalideException($"Second lot : {verification.Motif}");
        }

        
        // Le joueur a pris connaissance du bilan et veut continuer ; la manche suivante commence
        // quand les deux joueurs ont confirmé.
        
        public void CommencerMancheSuivante(int joueurId)
        {
            if (Etat != EtatPartie.FinDeManche) throw new ActionRefuseeException("Aucune manche n'attend d'être commencée.");
            if (!Confirmer(joueurId)) return;

            NumeroManche++;
            Etat = EtatPartie.EnCours;
            JoueurPouvantDeclarerNulle = null;
            // Décision Q3 (docs/REGLES.md) : les joueurs commencent les manches à tour de rôle.
            _premierJoueurDeLaManche = AutreJoueur(_premierJoueurDeLaManche);
            _historique.Add($"Manche {NumeroManche} commencée. {ObtenirJoueur(_premierJoueurDeLaManche).Nom} débute.");
            MemoriserStocksDebutManche();
            DonnerLaMain(_premierJoueurDeLaManche);
        }

        
        // Le joueur qui a récupéré toutes ses graines met fin à la partie : elle est déclarée nulle (2.5).
        
        public void DeclarerNulle(int joueurId)
        {
            if (Etat != EtatPartie.FinDeManche || JoueurPouvantDeclarerNulle != joueurId)
                throw new ActionRefuseeException("La nulle ne peut être déclarée que par le joueur qui a récupéré toutes ses graines, au bilan de manche.");
            JoueurPouvantDeclarerNulle = null;
            EstNulle = true;
            Etat = EtatPartie.Terminee;
            _confirmations.Clear();
            _historique.Add($"{ObtenirJoueur(joueurId).Nom} déclare la partie nulle.");
        }

        
        // Après la victoire, le joueur demande à rejouer ; quand les deux l'ont demandé, le plateau est
        // réinitialisé (12 trous actifs, 4 graines par trou, réceptacles vides) avec les mêmes joueurs (2.5).
        
        public void DemanderNouvellePartie(int joueurId)
        {
            if (Etat != EtatPartie.Terminee) throw new ActionRefuseeException("La partie n'est pas terminée.");
            if (!Confirmer(joueurId)) return;

            Plateau = new Plateau();
            NumeroManche = 1;
            VainqueurId = null;
            EstNulle = false;
            DernierBilan = null;
            Array.Clear(_coupsJoues);
            Array.Clear(_grainesCapturees);
            _historique.Clear();
            Etat = EtatPartie.EnAttente;
            Demarrer();
        }

        // Enregistre la confirmation du joueur ; vrai quand les deux joueurs ont confirmé.
        private bool Confirmer(int joueurId)
        {
            ObtenirJoueur(joueurId);
            _confirmations.Add(joueurId);
            if (_confirmations.Count < 2) return false;
            _confirmations.Clear();
            return true;
        }

        private void TerminerManche()
        {
            DernierBilan = _regles.Redistribuer(Plateau, NumeroManche);
            _confirmations.Clear();
            _historique.Add($"Fin de la manche {NumeroManche}.");
            int? rangeeGagnante = _regles.DeterminerVainqueur(Plateau);
            if (rangeeGagnante is int rangee)
            {
                Joueur vainqueur = JoueurDeLaRangee(rangee);
                VainqueurId = vainqueur.Id;
                Etat = EtatPartie.Terminee;
                _historique.Add($"{vainqueur.Nom} gagne la partie.");
                return;
            }
            Etat = EtatPartie.FinDeManche;
            int? rangeeNulle = _regles.RangeePouvantDeclarerNulle(_stocksDebutManche, DernierBilan);
            JoueurPouvantDeclarerNulle = rangeeNulle is int r ? JoueurDeLaRangee(r).Id : null;
        }

        private void MemoriserStocksDebutManche() =>
            _stocksDebutManche = [.. Enumerable.Range(0, 2).Select(r => Plateau.CompterGraines(r) + Plateau.ObtenirReceptacle(r).NombreGraines)];

        private Joueur JoueurDeLaRangee(int rangee) => rangee == Joueur1.NumeroRangee ? Joueur1 : Joueur2!;

        
        // Donne la main au joueur indiqué. S'il n'a aucun coup qui se termine (relais sans fin partout),
        // son tour passe à l'adversaire ; si aucun des deux ne peut jouer, la manche se termine.
        
        private void DonnerLaMain(int joueurId)
        {
            if (_regles.PeutJouer(Plateau, ObtenirJoueur(joueurId))) { JoueurActifId = joueurId; return; }
            int autre = AutreJoueur(joueurId);
            _historique.Add($"{ObtenirJoueur(joueurId).Nom} n'a aucun coup qui se termine : son tour passe.");
            // Le lot conservé ne sert que pendant le premier tour (2.1) : si ce tour passe sans « mort »,
            // le lot inutilisé rejoint le réceptacle du joueur, comme en fin de manche (Q5d).
            Plateau.RangerLotConserve(ObtenirJoueur(joueurId).NumeroRangee);
            if (_regles.PeutJouer(Plateau, ObtenirJoueur(autre))) { JoueurActifId = autre; return; }
            TerminerManche();
        }

        private static int AutreJoueur(int joueurId) => joueurId == 1 ? 2 : 1;
    }
}
