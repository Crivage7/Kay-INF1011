using System.Diagnostics;
using Kay.Api.Exceptions;
using Kay.Api.Models;

namespace Kay.Api.Services
{
    public class ReglesKay : IRegles
    {
        public const int GrainesParTrou = 4;
        // Graines d'un joueur au début de la partie (6 trous de 4).
        public const int GrainesParJoueur = Plateau.CasesParRangee * GrainesParTrou;
        // Un joueur perd s'il a au moins ce nombre de trous désactivés après redistribution
        public const int TrousDesactivesPourPerdre = 3;

        public virtual VarianteRegles Variante => VarianteRegles.Classique;

        /*
         Sens de distribution des graines : J1 (rangée 0) sème vers les
         positions croissantes, J2 (rangée 1) en sens inverse ; au bout d'une rangée, on passe dans
         la rangée adverse (11 → 0 pour J1, 0 → 11 pour J2), comme sur le plateau physique.
        */
        public static int Suivante(int position, int rangee) =>
            (position + (rangee == 0 ? 1 : Plateau.NombreCases - 1)) % Plateau.NombreCases;

        private static int Precedente(int position, int rangee) => Suivante(position, 1 - rangee);

        // Trou actif précédent dans le sens de distribution (le trou de départ est actif, la boucle se termine)
        private static int PrecedenteActive(Plateau plateau, int position, int rangee)
        {
            do position = Precedente(position, rangee);
            while (!plateau.ObtenirCase(position).EstActive);
            return position;
        }

        /*
         Ordre dans lequel les trous d'une rangée sont désactivés lorsque le stock ne suffit pas.
         « Premier trou » = celui à gauche du joueur, vu de sa place, soit la position 0 pour J1 
         et 6 pour J2 ; c'est l'ordre croissant des positions de chaque rangée.
        */
        private static IEnumerable<Case> OrdreDeDesactivation(Plateau plateau, int rangee) => plateau.CasesRangee(rangee);

        public Verification ChoixAutorise(Plateau plateau, Joueur joueur, int position)
        {
            if (position is < 0 or >= Plateau.NombreCases) return Verification.Refuse("Ce trou n'existe pas.");
            if (Plateau.RangeeDe(position) != joueur.NumeroRangee)
                return Verification.Refuse("Ce trou n'appartient pas à votre rangée.");
            Case choisie = plateau.ObtenirCase(position);
            if (!choisie.EstActive) return Verification.Refuse("Ce trou est désactivé pour cette manche.");
            if (choisie.NombreGraines == 0) return Verification.Refuse("Ce trou est vide.");
            return Verification.Autorise;
        }

        public ResultatDistribution DistribuerEtCapturer(Plateau plateau, Joueur joueur, int position) =>
            UtiliserLotConserve(plateau, joueur,
                DistribuerLot(plateau, joueur, position, plateau.ObtenirCase(position).RetirerToutesLesGraines()));

        // Premier tour avec deux lots
        public ResultatDistribution DistribuerDeuxLots(Plateau plateau, Joueur joueur, int position, int positionConservee)
        {
            // le second lot est retiré dès le choix et mis en réserve
            plateau.ConserverLot(joueur.NumeroRangee, positionConservee, plateau.ObtenirCase(positionConservee).RetirerToutesLesGraines());
            return DistribuerEtCapturer(plateau, joueur, position);
        }

        
        // Suite d'un coup pour un joueur qui garde un lot conservé
        
        private ResultatDistribution UtiliserLotConserve(Plateau plateau, Joueur joueur, ResultatDistribution resultat)
        {
            int rangee = joueur.NumeroRangee;
            if (plateau.ObtenirLotConserve(rangee) is null) return resultat;
            if (MancheTerminee(plateau))
            {
                // si le lot n'a pas été utilisé, il est placé au réceptacle du joueur.
                plateau.RangerLotConserve(rangee);
                return resultat;
            }
            // après une capture, le joueur rejoue et garde son lot en réserve.
            if (resultat.Issue == IssueDistribution.Capture) return resultat;
            // Le joueur est « mort » ; il sème son lot conservé à partir de son trou d'origine.
            // C'est la fin de ce lot (capture ou mort) qui décide de la suite du tour.
            LotConserve lot = plateau.RetirerLotConserve(rangee)!;
            ResultatDistribution second = DistribuerLot(plateau, joueur, lot.Position, lot.Graines);
            return second with { GrainesCapturees = resultat.GrainesCapturees + second.GrainesCapturees, LotConserveJoue = true };
        }

        // Sème des graines prises en main à partir du trou suivant, avec relais et captures.
        private static ResultatDistribution DistribuerLot(Plateau plateau, Joueur joueur, int depart, int graines)
        {
            int rangee = joueur.NumeroRangee;
            int curseur = depart;
            // États déjà vus aux points de relais : un état qui revient signifie un relais sans fin.
            var etatsVus = new HashSet<string>();
            while (true)
            {
                curseur = Semer(plateau, curseur, graines, rangee);
                Case arrivee = plateau.ObtenirCase(curseur);
                if (arrivee.NombreGraines == GrainesParTrou)
                    return new ResultatDistribution(IssueDistribution.Capture, Capturer(plateau, curseur, rangee));
                if (arrivee.NombreGraines == 1)
                    return new ResultatDistribution(IssueDistribution.Mort, 0);
                if (!etatsVus.Add(Empreinte(plateau, curseur)))
                    throw new CoupInvalideException("Ce coup ne se termine jamais (relais sans fin) : choisissez un autre trou.");
                graines = arrivee.RetirerToutesLesGraines();
            }
        }

        public bool PeutJouer(Plateau plateau, Joueur joueur) =>
            plateau.CasesRangee(joueur.NumeroRangee).Any(c =>
                ChoixAutorise(plateau, joueur, c.Position).EstAutorise && SeTermine(plateau, joueur, c.Position));

        public bool MancheTerminee(Plateau plateau) => plateau.RangeeVide(0) || plateau.RangeeVide(1);

        public BilanManche Redistribuer(Plateau plateau, int numeroManche)
        {
            var ramassees = new int[2];
            var stocks = new int[2];
            var actifs = new int[2];
            var reserves = new int[2];
            for (int rangee = 0; rangee < 2; rangee++)
            {
                // Chacun ramasse les graines restées dans sa rangée (la rangée vide n'en a aucune),
                // ainsi qu'un lot conservé encore en main.
                plateau.RangerLotConserve(rangee);
                Receptacle stock = plateau.ObtenirReceptacle(rangee);
                foreach (Case c in plateau.CasesRangee(rangee).Where(c => c.EstActive))
                    ramassees[rangee] += c.RetirerToutesLesGraines();
                stock.AjouterGraines(ramassees[rangee]);
                stocks[rangee] = stock.NombreGraines;

                // 4 graines par trou dans la limite de 6 ; les trous non remplis sont désactivés
                // à partir du premier trou de la rangée.
                actifs[rangee] = Math.Min(Plateau.CasesParRangee, stock.NombreGraines / GrainesParTrou);
                int desactives = Plateau.CasesParRangee - actifs[rangee];
                foreach (var (c, i) in OrdreDeDesactivation(plateau, rangee).Select((c, i) => (c, i)))
                {
                    if (i < desactives) c.Desactiver();
                    else c.ActiverAvecGraines(stock.RetirerGraines(GrainesParTrou));
                }
                reserves[rangee] = stock.NombreGraines;
            }
            return new BilanManche(numeroManche, ramassees, stocks, actifs, reserves);
        }

        public virtual int? DeterminerVainqueur(Plateau plateau) => RangeeGagnante(plateau, TrousDesactivesPourPerdre);
        
        // Rangée adverse de celle qui a au moins trous désactivés.
       
        protected static int? RangeeGagnante(Plateau plateau, int trousDesactivesPourPerdre)
        {
            var perdantes = Enumerable.Range(0, 2)
                .Where(r => plateau.CompterDesactivees(r) >= trousDesactivesPourPerdre)
                .ToList();
            Debug.Assert(perdantes.Count < 2, "Deux rangées perdantes : le total des graines n'est plus 48.");
            return perdantes.Count == 1 ? 1 - perdantes[0] : null;
        }

        /*
         Si le bilan revient à 24 graines chacun alors qu'un joueur en avait moins 
         au début de la manche, ce joueur a récupéré ses graines et peut déclarer la nulle.
        */
        public int? RangeePouvantDeclarerNulle(IReadOnlyList<int> stocksDebutManche, BilanManche bilan)
        {
            if (bilan.Stocks.Any(s => s != GrainesParJoueur)) return null;
            int rangee = Enumerable.Range(0, 2).FirstOrDefault(r => stocksDebutManche[r] < GrainesParJoueur, -1);
            return rangee < 0 ? null : rangee;
        }

        // Dépose les graines une à une à partir du trou suivant, en sautant les trous désactivés.
        private static int Semer(Plateau plateau, int curseur, int graines, int rangee)
        {
            while (graines > 0)
            {
                curseur = Suivante(curseur, rangee);
                Case cible = plateau.ObtenirCase(curseur);
                if (!cible.EstActive) continue;
                cible.AjouterGraine();
                graines--;
            }
            return curseur;
        }

        /*
            Capture le trou d'arrivée puis, en remontant, les trous précédents qui ont aussi 4 graines.
            Comme pour la distribution, les trous désactivés sont sautés.
        */
        private static int Capturer(Plateau plateau, int arrivee, int rangee)
        {
            int prises = 0;
            int p = arrivee;
            do
            {
                Case c = plateau.ObtenirCase(p);
                if (c.NombreGraines != GrainesParTrou) break;
                prises += c.RetirerToutesLesGraines();
                p = PrecedenteActive(plateau, p, rangee);
            } while (p != arrivee);
            plateau.ObtenirReceptacle(rangee).AjouterGraines(prises);
            return prises;
        }

        private bool SeTermine(Plateau plateau, Joueur joueur, int position)
        {
            try { DistribuerEtCapturer(plateau.Cloner(), joueur, position); return true; }
            catch (CoupInvalideException) { return false; }
        }

        private static string Empreinte(Plateau plateau, int curseur) =>
            curseur + ":" + string.Join(',', plateau.Cases.Select(c => c.NombreGraines));
    }
}
