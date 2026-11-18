import Icone from './Icone.jsx';

/**
 * Message d'erreur, affiché en bas de l'écran ; en cas de problème réseau, propose de relire
 * l'état de la partie (devis 3.1).
 */
export function BandeauErreur({ erreur, onRecharger, onFermer, occupe }) {
    if (!erreur) return null;
    return (
        <div role="alert" className="notification erreur">
            <span>{erreur.message}</span>
            {erreur.reseau && onRecharger && (
                <button type="button" className="bouton petit" onClick={onRecharger} disabled={occupe}>
                    <Icone nom="actualiser" taille={16} />Recharger l'état
                </button>
            )}
            <button type="button" className="bouton-icone" onClick={onFermer} aria-label="Fermer le message"><Icone nom="fermer" taille={18} /></button>
        </div>
    );
}

const MESSAGES_CONNEXION = {
    reconnexion: 'Connexion perdue : reconnexion en cours…',
    deconnecte: "Connexion temps réel interrompue : les coups de l'adversaire ne s'affichent plus.",
};

/** Signale une connexion temps réel perdue (la reconnexion SignalR est automatique). */
export function BandeauConnexion({ connexion, onRecharger, occupe }) {
    const message = MESSAGES_CONNEXION[connexion];
    if (!message) return null;
    return (
        <div role="status" className="bandeau avertissement">
            <span>{message}</span>
            <button type="button" className="bouton petit" onClick={onRecharger} disabled={occupe}>
                <Icone nom="actualiser" taille={16} />Recharger l'état
            </button>
        </div>
    );
}
