import { describe, expect, it } from 'vitest';
import {
    grainesDuJoueur, initiales, positionsGraines, receptaclesEnHausse, sensDuSemis, tempsEcoule, trousDesactives, trousModifies,
} from './affichage.js';

const etat = (graines, receptacles = [0, 0], code = 'A') => ({
    code,
    receptacles,
    cases: graines.map((g, position) => ({ position, nombreGraines: Math.max(g, 0), estActive: g >= 0 })),
});

describe('initiales', () => {
    it.each([
        ['Alice', 'AL'],
        ['Jean Pierre', 'JP'],
        ['  émile  zola ', 'ÉZ'],
        ['', '?'],
    ])('%s → %s', (nom, attendu) => expect(initiales(nom)).toBe(attendu));
});

describe('changements entre deux états', () => {
    const avant = etat([4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4]);

    it('repère les trous dont le contenu ou l’état a changé', () => {
        const apres = etat([0, 5, 5, 5, 5, 4, 4, 4, 4, 4, 4, -1]);
        expect([...trousModifies(avant, apres)]).toEqual([0, 1, 2, 3, 4, 11]);
    });

    it("ne signale rien pour un premier affichage ou une autre partie", () => {
        expect(trousModifies(null, avant).size).toBe(0);
        expect(trousModifies(avant, etat([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [0, 0], 'B')).size).toBe(0);
    });

    it('signale les réceptacles qui augmentent (capture)', () => {
        expect(receptaclesEnHausse(etat([], [4, 8]), etat([], [8, 8]))).toEqual([true, false]);
    });
});

describe('sensDuSemis', () => {
    it('J1 sème vers la droite dans la rangée du bas, puis vers la gauche en haut', () => {
        expect(sensDuSemis(1)).toEqual({ bas: 'droite', haut: 'gauche' });
    });

    it('J2 sème dans le sens contraire', () => {
        expect(sensDuSemis(2)).toEqual({ bas: 'gauche', haut: 'droite' });
    });
});

describe('comptages par joueur', () => {
    const partie = etat([4, 0, -1, 4, 4, 4, 1, 1, 1, 1, 1, 1], [3, 20]);

    it('additionne la rangée et le réceptacle', () => {
        expect(grainesDuJoueur(partie, 1)).toBe(19);
        expect(grainesDuJoueur(partie, 2)).toBe(26);
    });

    it('compte le lot conservé en réserve avec les graines de son joueur', () => {
        const avecLot = { ...partie, lotConserve: { joueurId: 1, position: 1, graines: 4 } };
        expect(grainesDuJoueur(avecLot, 1)).toBe(23);
        expect(grainesDuJoueur(avecLot, 2)).toBe(26);
    });

    it('compte les trous désactivés', () => {
        expect(trousDesactives(partie, 1)).toBe(1);
        expect(trousDesactives(partie, 2)).toBe(0);
    });
});

describe('tempsEcoule', () => {
    const maintenant = Date.parse('2026-11-10T12:00:00Z');
    it.each([
        ['2026-11-10T11:59:30Z', "à l'instant"],
        ['2026-11-10T11:57:00Z', 'il y a 3 min'],
        ['2026-11-10T09:30:00Z', 'il y a 2 h'],
    ])('%s → %s', (iso, attendu) => expect(tempsEcoule(iso, maintenant)).toBe(attendu));
});

describe('positionsGraines', () => {
    it('place une graine seule au centre', () => {
        expect(positionsGraines(1)).toEqual([{ x: 50, y: 50 }]);
    });

    it('dessine au plus le maximum, toutes dans le trou', () => {
        const positions = positionsGraines(40, 14);
        expect(positions).toHaveLength(14);
        for (const { x, y } of positions) expect(Math.hypot(x - 50, y - 50)).toBeLessThanOrEqual(30);
    });

    it('est stable d’un rendu à l’autre', () => {
        expect(positionsGraines(6)).toEqual(positionsGraines(6));
    });
});
