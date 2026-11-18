using System.Collections.Concurrent;
using System.Security.Cryptography;
using Kay.Api.DTOs;
using Kay.Api.Exceptions;
using Kay.Api.Models;

namespace Kay.Api.Services
{
    public class PartieService
    {
        // Une partie en attente plus ancienne n'est plus proposée au pairage
        private static readonly TimeSpan DureeAffichageEnAttente = TimeSpan.FromMinutes(30);
        private const int NombreMaximumEnAttente = 20;

        private sealed class Session
        {
            public readonly object Verrou = new();
            public Partie Partie { get; }
            public DateTime CreeeLe { get; } = DateTime.UtcNow;
            public string Jeton1 { get; } = GenererSecret();
            public string? Jeton2 { get; set; }
            // Incrémentée à chaque modification : le client ignore un état diffusé plus ancien que celui qu'il affiche.
            public long Version { get; set; } = 1;
            // Vrai une fois le résultat de la partie terminée enregistré ; remis à faux quand on rejoue.
            public bool ResultatEnregistre { get; set; }
            public Session(Partie partie) { Partie = partie; }

            // Joueurs représentés par le jeton : un seul en ligne, les deux en mode local.
            public IReadOnlySet<int> JoueursDuJeton(string jeton)
            {
                if (jeton == Jeton1) return Partie.Mode == ModePartie.Local ? new HashSet<int> { 1, 2 } : new HashSet<int> { 1 };
                if (Jeton2 is not null && jeton == Jeton2) return new HashSet<int> { 2 };
                return new HashSet<int>();
            }

            public PartieDto VersDto() => PartieDto.Depuis(Partie, Version);
        }

        private readonly ConcurrentDictionary<string, Session> _sessions = new();
        // Une stratégie de règles par variante, toutes fournies par l'injection de dépendances.
        private readonly IReadOnlyDictionary<VarianteRegles, IRegles> _regles;
        private readonly IDepotResultats _depot;

        public PartieService(IEnumerable<IRegles> regles, IDepotResultats depot)
        {
            _regles = regles.ToDictionary(r => r.Variante);
            _depot = depot;
        }

        private static string GenererSecret() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        private Session Trouver(string code) => _sessions.TryGetValue(code.ToUpperInvariant(), out var s)
            ? s : throw new PartieIntrouvableException(code);

        /*
         Crée une partie. En ligne, elle attend un adversaire ; en mode local, les deux joueurs
         partagent l'écran et le même jeton, et la partie démarre aussitôt.
        */
        public AccesPartieReponse Creer(string nom, VarianteRegles variante = VarianteRegles.Classique,
            ModePartie mode = ModePartie.EnLigne, string? nomJoueur2 = null)
        {
            if (!_regles.TryGetValue(variante, out IRegles? regles))
                throw new ActionRefuseeException("Variante de règles inconnue.");
            if (mode == ModePartie.Local && string.IsNullOrWhiteSpace(nomJoueur2))
                throw new ActionRefuseeException("Le nom du deuxième joueur est requis en mode local.");
            for (int n = 0; n < 10; n++)
            {
                string code = Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
                var partie = new Partie(code, nom, regles, mode);
                if (mode == ModePartie.Local)
                {
                    partie.AjouterAdversaire(nomJoueur2!);
                    partie.Demarrer();
                }
                var s = new Session(partie);
                if (_sessions.TryAdd(code, s)) return new(s.VersDto(), 1, s.Jeton1);
            }
            throw new InvalidOperationException("Impossible de créer un code de partie.");
        }

        // Parties en ligne récentes qui attendent un adversaire (écran de pairage)
        public IReadOnlyList<PartieEnAttenteDto> PartiesEnAttente()
        {
            DateTime limite = DateTime.UtcNow - DureeAffichageEnAttente;
            return _sessions.Values
                .Where(s => s.CreeeLe > limite)
                .Select(s => { lock (s.Verrou) return (s.CreeeLe, s.Partie.Etat, s.Partie.Mode, Dto: PartieEnAttenteDto.Depuis(s.Partie, s.CreeeLe)); })
                .Where(s => s.Etat == EtatPartie.EnAttente && s.Mode == ModePartie.EnLigne)
                .OrderByDescending(s => s.CreeeLe)
                .Take(NombreMaximumEnAttente)
                .Select(s => s.Dto)
                .ToList();
        }

        public AccesPartieReponse Rejoindre(string code, string nom)
        {
            var s = Trouver(code);
            lock (s.Verrou)
            {
                s.Partie.AjouterAdversaire(nom);
                s.Partie.Demarrer();
                s.Jeton2 = GenererSecret();
                s.Version++;
                return new(s.VersDto(), 2, s.Jeton2);
            }
        }

        // Le créateur renonce à une partie qui attend encore son adversaire.
        public void Annuler(string code, string jeton)
        {
            var s = Trouver(code);
            lock (s.Verrou)
            {
                if (!s.JoueursDuJeton(jeton).Contains(1)) throw new AccesRefuseException("Seul le créateur peut annuler la partie.");
                if (s.Partie.Etat != EtatPartie.EnAttente) throw new ActionRefuseeException("La partie a déjà commencé.");
                _sessions.TryRemove(s.Partie.Code, out _);
            }
        }

        public PartieDto Lire(string code) { var s = Trouver(code); lock (s.Verrou) return s.VersDto(); }

        public PartieDto Jouer(string code, string jeton, int position, int? positionConservee = null) =>
            Executer(code, jeton, (partie, joueurs) =>
                partie.JouerCase(Choisir(joueurs, partie.JoueurActifId), position, positionConservee));

        public PartieDto Continuer(string code, string jeton) =>
            Executer(code, jeton, (partie, joueurs) =>
            {
                foreach (int id in joueurs) partie.CommencerMancheSuivante(id);
            });

        public PartieDto DeclarerNulle(string code, string jeton) =>
            Executer(code, jeton, (partie, joueurs) =>
                partie.DeclarerNulle(Choisir(joueurs, partie.JoueurPouvantDeclarerNulle)));

        public PartieDto Rejouer(string code, string jeton) =>
            Executer(code, jeton, (partie, joueurs) =>
            {
                foreach (int id in joueurs) partie.DemanderNouvellePartie(id);
            });

        public bool JetonValide(string code, string jeton)
        {
            if (!_sessions.TryGetValue(code.ToUpperInvariant(), out var s)) return false;
            lock (s.Verrou) return s.JoueursDuJeton(jeton).Count > 0;
        }

        // Identifie le(s) joueur(s) par le jeton puis applique l'action sous le verrou de la session.
        private PartieDto Executer(string code, string jeton, Action<Partie, IReadOnlySet<int>> action)
        {
            var s = Trouver(code);
            lock (s.Verrou)
            {
                var joueurs = s.JoueursDuJeton(jeton);
                if (joueurs.Count == 0) throw new AccesRefuseException("Jeton incorrect : vous ne participez pas à cette partie.");
                action(s.Partie, joueurs);
                s.Version++;
                EnregistrerSiTerminee(s);
                return s.VersDto();
            }
        }

        // Conserve le résultat pour le tableau d'honneur, une seule fois par partie terminée.
        private void EnregistrerSiTerminee(Session s)
        {
            if (!s.Partie.EstTerminee) { s.ResultatEnregistre = false; return; }
            if (s.ResultatEnregistre) return;
            _depot.Enregistrer(ResultatPartie.Depuis(s.Partie, DateTime.UtcNow));
            s.ResultatEnregistre = true;
        }

        // Le joueur concerné s'il est représenté par le jeton, sinon un joueur du jeton (la Partie refusera).
        private static int Choisir(IReadOnlySet<int> joueurs, int? concerne) =>
            concerne is int id && joueurs.Contains(id) ? id : joueurs.First();
    }
}
