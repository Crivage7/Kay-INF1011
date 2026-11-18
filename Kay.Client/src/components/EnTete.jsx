const ETATS_CONNEXION = {
    connexion: { classe: 'attente', libelle: 'Connexion…' },
    connecte: { classe: 'ok', libelle: 'En ligne' },
    reconnexion: { classe: 'attente', libelle: 'Reconnexion…' },
    deconnecte: { classe: 'ko', libelle: 'Hors ligne' },
};

/** Barre supérieure : marque (retour à l'accueil hors partie) et état de la connexion temps réel. */
export default function EnTete({ onAccueil, connexion }) {
    const etat = connexion ? ETATS_CONNEXION[connexion] : null;
    return (
        <header className="entete">
            <div className="entete-contenu">
                {onAccueil
                    ? <button type="button" className="marque" onClick={onAccueil} aria-label="Kay, retour à l'accueil"><span className="marque-logo" aria-hidden="true" />Kay</button>
                    : <span className="marque"><span className="marque-logo" aria-hidden="true" />Kay</span>}
                {etat && <span className={`connexion ${etat.classe}`} role="status"><span className="point" aria-hidden="true" />{etat.libelle}</span>}
            </div>
        </header>
    );
}
