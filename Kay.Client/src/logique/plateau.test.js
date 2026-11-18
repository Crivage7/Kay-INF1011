import { describe, expect, it } from 'vitest';
import { dispositionPlateau, estMonTour, numeroDuTrou, perspective, rangeeDe, trouJouable } from './plateau.js';

const cases = graines => graines.map((g, position) => ({ position, nombreGraines: Math.max(g, 0), estActive: g >= 0 }));
const partie = (champs = {}) => ({
    mode: 'EnLigne', etat: 'EnCours', joueurActifId: 1,
    cases: cases([4, 0, -1, 4, 4, 4, 4, 4, 4, 4, 4, 4]),
    ...champs,
});

describe('dispositionPlateau', () => {
    it('place la rangée de J1 en bas et celle de J2 en haut, lue de droite à gauche (parcours réel)', () => {
        expect(dispositionPlateau(1)).toEqual({ haut: [11, 10, 9, 8, 7, 6], bas: [0, 1, 2, 3, 4, 5] });
    });

    it('vu par J2 : sa rangée en bas, de sa gauche (6) à sa droite (11)', () => {
        expect(dispositionPlateau(2)).toEqual({ haut: [5, 4, 3, 2, 1, 0], bas: [6, 7, 8, 9, 10, 11] });
    });

    it('le semis suit le tour du plateau : après 5 vient 6, juste au-dessus', () => {
        const { haut, bas } = dispositionPlateau(1);
        expect(bas.at(-1)).toBe(5);
        expect(haut.at(-1)).toBe(6);
    });
});

describe('perspective et tour', () => {
    it('en ligne, chacun voit sa rangée ; en local, J1 est toujours en bas', () => {
        expect(perspective(partie(), { joueurId: 2 })).toBe(2);
        expect(perspective(partie({ mode: 'Local' }), { joueurId: 1 })).toBe(1);
    });

    it("en ligne, c'est mon tour seulement si je suis le joueur actif", () => {
        expect(estMonTour(partie(), { joueurId: 1 })).toBe(true);
        expect(estMonTour(partie(), { joueurId: 2 })).toBe(false);
    });

    it("en local, l'écran joue pour le joueur actif, quel qu'il soit", () => {
        expect(estMonTour(partie({ mode: 'Local', joueurActifId: 2 }), { joueurId: 1 })).toBe(true);
    });

    it('personne ne joue pendant un bilan', () => {
        expect(estMonTour(partie({ etat: 'FinDeManche' }), { joueurId: 1 })).toBe(false);
    });
});

describe('trouJouable', () => {
    const session = { joueurId: 1 };

    it('accepte un trou actif et non vide de la rangée du joueur actif', () => {
        expect(trouJouable(partie(), session, 0)).toBe(true);
    });

    it.each([
        [1, 'vide'],
        [2, 'désactivé'],
        [6, 'dans la rangée adverse'],
    ])('refuse le trou %i (%s)', position => {
        expect(trouJouable(partie(), session, position)).toBe(false);
    });

    it("refuse tout trou quand ce n'est pas mon tour", () => {
        expect(trouJouable(partie({ joueurActifId: 2 }), session, 6)).toBe(false);
    });
});

describe('numérotation', () => {
    it('rangée et numéro de trou vu par son propriétaire', () => {
        expect(rangeeDe(5)).toBe(0);
        expect(rangeeDe(6)).toBe(1);
        expect(numeroDuTrou(0)).toBe(1);
        expect(numeroDuTrou(11)).toBe(6);
    });
});
