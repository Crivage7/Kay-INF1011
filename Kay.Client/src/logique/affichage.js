// Calculs d'affichage purs (sans React) : avatars, changements entre deux états, durées, graines.

/** Deux lettres pour l'avatar d'un joueur : initiales des deux premiers mots, sinon les deux premières lettres. */
export function initiales(nom) {
    const mots = (nom ?? '').trim().split(/\s+/).filter(Boolean);
    if (mots.length === 0) return '?';
    const lettres = mots.length > 1 ? mots[0][0] + mots[1][0] : mots[0].slice(0, 2);
    return lettres.toLocaleUpperCase('fr');
}

/** Positions des trous dont le contenu ou l'état a changé entre deux états d'une même partie. */
export function trousModifies(avant, apres) {
    if (!avant || !apres || avant.code !== apres.code) return new Set();
    return new Set(apres.cases
        .filter(c => {
            const ancien = avant.cases[c.position];
            return ancien.nombreGraines !== c.nombreGraines || ancien.estActive !== c.estActive;
        })
        .map(c => c.position));
}

/** Réceptacles (index 0 = joueur 1) dont le contenu a augmenté : une capture vient d'avoir lieu. */
export function receptaclesEnHausse(avant, apres) {
    if (!avant || !apres || avant.code !== apres.code) return [false, false];
    return apres.receptacles.map((n, i) => n > avant.receptacles[i]);
}

/**
 * Sens du semis du joueur actif, à l'écran. Chaque joueur voit sa rangée en bas, positions croissantes
 * de gauche à droite ; la rangée du haut est inversée. J1 sème vers les positions croissantes, J2 à
 * l'inverse : le sens à l'écran ne dépend donc que du joueur actif.
 */
export function sensDuSemis(joueurActifId) {
    return joueurActifId === 1 ? { bas: 'droite', haut: 'gauche' } : { bas: 'gauche', haut: 'droite' };
}

/** Graines d'un joueur : trous de sa rangée, réceptacle et lot conservé en réserve au premier tour. */
export function grainesDuJoueur(partie, joueurId) {
    const rangee = partie.cases.filter(c => Math.floor(c.position / 6) === joueurId - 1);
    const lot = partie.lotConserve?.joueurId === joueurId ? partie.lotConserve.graines : 0;
    return rangee.reduce((total, c) => total + c.nombreGraines, 0) + partie.receptacles[joueurId - 1] + lot;
}

export const trousDesactives = (partie, joueurId) =>
    partie.cases.filter(c => Math.floor(c.position / 6) === joueurId - 1 && !c.estActive).length;

/** Durée écoulée lisible (« à l'instant », « il y a 3 min », « il y a 2 h »). */
export function tempsEcoule(iso, maintenant = Date.now()) {
    const secondes = Math.max(0, Math.round((maintenant - new Date(iso).getTime()) / 1000));
    if (secondes < 60) return "à l'instant";
    const minutes = Math.floor(secondes / 60);
    if (minutes < 60) return `il y a ${minutes} min`;
    return `il y a ${Math.floor(minutes / 60)} h`;
}

const ANGLE_OR = Math.PI * (3 - Math.sqrt(5));

/**
 * Positions (en % du trou) des graines dessinées : spirale de l'angle d'or, régulière et stable
 * d'un rendu à l'autre. Au-delà de `maximum`, seules `maximum` graines sont dessinées.
 */
export function positionsGraines(nombre, maximum = 14) {
    const n = Math.min(nombre, maximum);
    return Array.from({ length: n }, (_, i) => {
        const rayon = n === 1 ? 0 : 30 * Math.sqrt((i + 0.5) / n);
        const angle = i * ANGLE_OR;
        return { x: 50 + rayon * Math.cos(angle), y: 50 + rayon * Math.sin(angle) };
    });
}
