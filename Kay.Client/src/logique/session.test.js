import { describe, expect, it } from 'vitest';
import { chargerSession, effacerSession, enregistrerSession } from './session.js';

function stockageMemoire() {
    const valeurs = new Map();
    return {
        getItem: cle => valeurs.get(cle) ?? null,
        setItem: (cle, valeur) => valeurs.set(cle, valeur),
        removeItem: cle => valeurs.delete(cle),
    };
}

describe('session', () => {
    it('survit à un rechargement : enregistrer puis charger', () => {
        const stockage = stockageMemoire();
        const session = { code: 'ABCD1234', jeton: 'secret', joueurId: 2 };
        enregistrerSession(session, stockage);
        expect(chargerSession(stockage)).toEqual(session);
    });

    it('effacer supprime la session', () => {
        const stockage = stockageMemoire();
        enregistrerSession({ code: 'A', jeton: 'b', joueurId: 1 }, stockage);
        effacerSession(stockage);
        expect(chargerSession(stockage)).toBeNull();
    });

    it('ignore une valeur corrompue ou incomplète', () => {
        const stockage = stockageMemoire();
        stockage.setItem('kay.session', '{pas du json');
        expect(chargerSession(stockage)).toBeNull();
        stockage.setItem('kay.session', JSON.stringify({ code: 'A' }));
        expect(chargerSession(stockage)).toBeNull();
    });

    it('fonctionne sans stockage disponible (navigation privée)', () => {
        const indisponible = { getItem: () => { throw new Error(); }, setItem: () => { throw new Error(); }, removeItem: () => { throw new Error(); } };
        expect(() => enregistrerSession({ code: 'A', jeton: 'b', joueurId: 1 }, indisponible)).not.toThrow();
        expect(chargerSession(indisponible)).toBeNull();
        expect(() => effacerSession(indisponible)).not.toThrow();
    });
});
