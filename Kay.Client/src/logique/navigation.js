// Choix de l'écran à partir de l'état de la partie reçu du serveur.

export const ECRANS = Object.freeze({
    accueil: 'accueil',
    regles: 'regles',
    configuration: 'configuration',
    pairage: 'pairage',
    partie: 'partie',
    bilan: 'bilan',
    resultat: 'resultat',
    tableauHonneur: 'tableau-honneur',
});

/**
 * Empreinte d'une fin de partie : permet de savoir si le bilan final a déjà été consulté,
 * sans confondre deux parties successives jouées avec le même code.
 */
export const empreinteFin = partie =>
    `${partie.code}:${partie.vainqueurId ?? 'nulle'}:${partie.numeroManche}:${partie.coupsJoues.join('-')}`;

/**
 * Écran imposé par la partie en cours, ou null si aucune partie n'est active.
 * En fin de partie, le bilan de la dernière manche est montré avant le résultat (devis, section 6),
 * sauf après une nulle, déclarée depuis le bilan lui-même.
 */
export function ecranDeLaPartie(partie, finConsultee) {
    if (!partie) return null;
    switch (partie.etat) {
        case 'EnAttente': return ECRANS.pairage;
        case 'EnCours': return ECRANS.partie;
        case 'FinDeManche': return ECRANS.bilan;
        case 'Terminee':
            return partie.estNulle || !partie.dernierBilan || finConsultee === empreinteFin(partie)
                ? ECRANS.resultat
                : ECRANS.bilan;
        default: return ECRANS.partie;
    }
}

export const nomJoueur = (partie, id) => partie?.joueurs.find(j => j.id === id)?.nom ?? `Joueur ${id}`;

export const LIBELLES_VARIANTE = Object.freeze({
    Classique: 'Classique',
    CouperLaTete: 'Couper la tête',
});
