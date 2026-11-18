namespace Kay.Api.Models
{
    public class Plateau
    {
        public const int NombreCases = 12;
        public const int CasesParRangee = 6;
        public const int GrainesInitialesParCase = 4;

        private readonly List<Case> _cases = Enumerable.Range(0, NombreCases).Select(i => new Case(i, GrainesInitialesParCase)).ToList();
        private readonly List<Receptacle> _receptacles = new() { new(), new() };
        // Lot conservé « en main » au premier tour (2.1), indexé par rangée ; null si aucun.
        private readonly LotConserve?[] _lotsConserves = new LotConserve?[2];
        public IReadOnlyList<Case> Cases => _cases.AsReadOnly();
        public IReadOnlyList<Receptacle> Receptacles => _receptacles.AsReadOnly();

        // Construit un plateau dans un état donné (-1 = trou désactivé)
        internal static Plateau Depuis(int[] graines, int receptacle1 = 0, int receptacle2 = 0)
        {
            if (graines.Length != NombreCases) throw new ArgumentException("Il faut 12 trous.", nameof(graines));
            var plateau = new Plateau();
            for (int i = 0; i < NombreCases; i++)
            {
                plateau._cases[i].ActiverAvecGraines(Math.Max(0, graines[i]));
                if (graines[i] < 0) plateau._cases[i].Desactiver();
            }
            plateau._receptacles[0].AjouterGraines(receptacle1);
            plateau._receptacles[1].AjouterGraines(receptacle2);
            return plateau;
        }

        // Copie indépendante, utilisée pour simuler un coup sans risquer l'état réel
        public Plateau Cloner()
        {
            var copie = Depuis(
                _cases.Select(c => c.EstActive ? c.NombreGraines : -1).ToArray(),
                _receptacles[0].NombreGraines,
                _receptacles[1].NombreGraines);
            Array.Copy(_lotsConserves, copie._lotsConserves, _lotsConserves.Length);
            return copie;
        }

        public static int RangeeDe(int position) => position / CasesParRangee;

        public Case ObtenirCase(int position)
        {
            if (position is < 0 or >= NombreCases) throw new ArgumentOutOfRangeException(nameof(position));
            return _cases[position];
        }
        public Receptacle ObtenirReceptacle(int rangee)
        {
            if (rangee is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(rangee));
            return _receptacles[rangee];
        }
        // Cases d'une rangée, par position croissante (0..5 ou 6..11).
        public IEnumerable<Case> CasesRangee(int rangee)
        {
            if (rangee is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(rangee));
            return _cases.Skip(rangee * CasesParRangee).Take(CasesParRangee);
        }

        public LotConserve? ObtenirLotConserve(int rangee)
        {
            if (rangee is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(rangee));
            return _lotsConserves[rangee];
        }

        // Met en main les graines prises dans le trou de la rangée.
        public void ConserverLot(int rangee, int position, int graines)
        {
            if (ObtenirLotConserve(rangee) is not null) throw new InvalidOperationException("Un lot est déjà conservé.");
            if (RangeeDe(ObtenirCase(position).Position) != rangee) throw new ArgumentOutOfRangeException(nameof(position));
            if (graines < 0) throw new ArgumentOutOfRangeException(nameof(graines));
            _lotsConserves[rangee] = new LotConserve(position, graines);
        }

        // Reprend le lot conservé de la rangée pour le semer ; null si aucun.
        public LotConserve? RetirerLotConserve(int rangee)
        {
            LotConserve? lot = ObtenirLotConserve(rangee);
            _lotsConserves[rangee] = null;
            return lot;
        }

        // Un lot conservé qui ne sera pas semé rejoint le réceptacle de son joueur (aucune graine ne disparaît).
        public void RangerLotConserve(int rangee)
        {
            if (RetirerLotConserve(rangee) is LotConserve lot) _receptacles[rangee].AjouterGraines(lot.Graines);
        }

        // Total des graines en jeu (trous, réceptacles et lot conservé) ; vaut toujours 48
        public int CompterGraines() =>
            _cases.Sum(c => c.NombreGraines) + _receptacles.Sum(r => r.NombreGraines) + _lotsConserves.Sum(l => l?.Graines ?? 0);
        // Graines présentes dans les trous d'une rangée.
        public int CompterGraines(int rangee) => CasesRangee(rangee).Sum(c => c.NombreGraines);
        public int CompterDesactivees(int rangee) => CasesRangee(rangee).Count(c => !c.EstActive);
        public bool RangeeVide(int rangee) => CompterGraines(rangee) == 0;
    }

    // Second lot du premier tour, gardé en réserve : graines en main et trou d'où elles seront semées.
    public sealed record LotConserve(int Position, int Graines);
}
