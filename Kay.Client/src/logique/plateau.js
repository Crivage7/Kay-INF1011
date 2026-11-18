// Logique d'affichage du plateau, alignée sur le parcours réel des graines côté serveur
// (docs/REGLES.md, Q1) : J1 (rangée 0, positions 0..5) sème vers la droite puis continue dans la
// rangée de J2 (positions 6..11) de droite à gauche. La rangée du haut est donc affichée inversée.

const RANGEE_J1 = [0, 1, 2, 3, 4, 5];
const RANGEE_J2 = [6, 7, 8, 9, 10, 11];

export const rangeeDe = position => Math.floor(position / 6);

/** Positions à afficher, de gauche à droite : la rangée du joueur `vuPar` est en bas. */
export function dispositionPlateau(vuPar) {
    if (vuPar === 2) return { haut: [...RANGEE_J1].reverse(), bas: [...RANGEE_J2] };
    return { haut: [...RANGEE_J2].reverse(), bas: [...RANGEE_J1] };
}

/** Joueur dont la rangée est affichée en bas : le sien en ligne, toujours J1 sur un écran partagé. */
export const perspective = (partie, session) => (partie.mode === 'Local' ? 1 : session.joueurId);

/** Vrai si cet écran peut jouer maintenant (en mode local, l'écran joue pour les deux). */
export const estMonTour = (partie, session) =>
    partie.etat === 'EnCours' && (partie.mode === 'Local' || partie.joueurActifId === session.joueurId);

/** Vrai si le trou peut être choisi par le joueur actif depuis cet écran. */
export function trouJouable(partie, session, position) {
    const trou = partie.cases[position];
    return estMonTour(partie, session)
        && rangeeDe(position) === partie.joueurActifId - 1
        && trou.estActive
        && trou.nombreGraines > 0;
}

/** Numéro du trou vu par son propriétaire (1 à 6, de sa gauche vers sa droite). */
export const numeroDuTrou = position => (position % 6) + 1;
