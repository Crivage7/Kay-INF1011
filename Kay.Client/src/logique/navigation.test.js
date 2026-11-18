import { describe, expect, it } from 'vitest';
import { ECRANS, ecranDeLaPartie, empreinteFin, nomJoueur } from './navigation.js';

const partie = (champs = {}) => ({
    code: 'ABCD1234', etat: 'EnCours', numeroManche: 2, vainqueurId: null, estNulle: false,
    coupsJoues: [10, 9], dernierBilan: { numeroManche: 1 },
    joueurs: [{ id: 1, nom: 'Alice' }, { id: 2, nom: 'Bob' }],
    ...champs,
});

describe('ecranDeLaPartie', () => {
    it("sans partie, aucun écran n'est imposé", () => {
        expect(ecranDeLaPartie(null, null)).toBeNull();
    });

    it.each([
        ['EnAttente', ECRANS.pairage],
        ['EnCours', ECRANS.partie],
        ['FinDeManche', ECRANS.bilan],
    ])("l'état %s mène à l'écran %s", (etat, ecran) => {
        expect(ecranDeLaPartie(partie({ etat }), null)).toBe(ecran);
    });

    it('en fin de partie, le bilan de la dernière manche est montré avant le résultat', () => {
        const terminee = partie({ etat: 'Terminee', vainqueurId: 1 });
        expect(ecranDeLaPartie(terminee, null)).toBe(ECRANS.bilan);
        expect(ecranDeLaPartie(terminee, empreinteFin(terminee))).toBe(ECRANS.resultat);
    });

    it('après une nulle déclarée depuis le bilan, on passe directement au résultat', () => {
        expect(ecranDeLaPartie(partie({ etat: 'Terminee', estNulle: true }), null)).toBe(ECRANS.resultat);
    });

    it("le bilan consulté d'une partie précédente ne compte pas pour la suivante", () => {
        const premiere = partie({ etat: 'Terminee', vainqueurId: 1 });
        const seconde = partie({ etat: 'Terminee', vainqueurId: 2, coupsJoues: [4, 5] });
        expect(ecranDeLaPartie(seconde, empreinteFin(premiere))).toBe(ECRANS.bilan);
    });
});

describe('nomJoueur', () => {
    it('donne le nom du joueur, ou un libellé par défaut', () => {
        expect(nomJoueur(partie(), 2)).toBe('Bob');
        expect(nomJoueur(partie({ joueurs: [{ id: 1, nom: 'Alice' }] }), 2)).toBe('Joueur 2');
    });
});
