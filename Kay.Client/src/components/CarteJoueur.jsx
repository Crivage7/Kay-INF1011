import { grainesDuJoueur, initiales, trousDesactives } from '../logique/affichage.js';
import { nomJoueur } from '../logique/navigation.js';

/** Panneau d'un joueur pendant la partie : identité, tour, graines en sa possession. */
export default function CarteJoueur({ partie, joueurId, role, estMoi = false }) {
    const nom = nomJoueur(partie, joueurId);
    const actif = partie.etat === 'EnCours' && partie.joueurActifId === joueurId;
    const graines = grainesDuJoueur(partie, joueurId);
    const desactives = trousDesactives(partie, joueurId);
    return (
        <div className={`carte-joueur joueur-${joueurId}${actif ? ' actif' : ''}`}>
            <span className="avatar" aria-hidden="true">{initiales(nom)}</span>
            <div className="carte-joueur-identite">
                <strong>{nom}</strong>
                <small>{role}{desactives > 0 && ` · ${desactives} trou${desactives > 1 ? 's' : ''} désactivé${desactives > 1 ? 's' : ''}`}</small>
            </div>
            {actif && <span className="pastille-tour">{estMoi ? 'À vous' : partie.mode === 'Local' ? 'À jouer' : 'Joue…'}</span>}
            <div className="carte-joueur-graines" title="Graines dans sa rangée et son réceptacle">
                <strong>{graines}</strong>
                <small>graines</small>
            </div>
        </div>
    );
}
