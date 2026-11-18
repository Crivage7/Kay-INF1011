import { afterEach, describe, expect, it, vi } from 'vitest';
import { api, appeler, ErreurApi } from './client.js';
import { plusRecente } from '../hooks/usePartie.js';

const reponse = (statut, corps) => ({
    ok: statut >= 200 && statut < 300,
    status: statut,
    json: () => (corps === undefined ? Promise.reject(new Error('vide')) : Promise.resolve(corps)),
});

afterEach(() => vi.unstubAllGlobals());

describe('appeler', () => {
    it('envoie le JSON et renvoie la réponse', async () => {
        const fetch = vi.fn().mockResolvedValue(reponse(200, { code: 'X' }));
        vi.stubGlobal('fetch', fetch);
        await expect(appeler('/api/parties', { methode: 'POST', donnees: { nom: 'Alice' } })).resolves.toEqual({ code: 'X' });
        expect(fetch).toHaveBeenCalledWith('/api/parties', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: '{"nom":"Alice"}',
        });
    });

    it('transmet le message explicatif du serveur (coup invalide)', async () => {
        vi.stubGlobal('fetch', vi.fn().mockResolvedValue(reponse(400, { erreur: 'Ce trou est vide.' })));
        const erreur = await appeler('/api/parties/X/jouer', { methode: 'POST', donnees: {} }).catch(e => e);
        expect(erreur).toBeInstanceOf(ErreurApi);
        expect(erreur.message).toBe('Ce trou est vide.');
        expect(erreur.statut).toBe(400);
        expect(erreur.reseau).toBe(false);
    });

    it('signale une erreur réseau quand le serveur est injoignable', async () => {
        vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')));
        const erreur = await appeler('/api/parties/X').catch(e => e);
        expect(erreur.reseau).toBe(true);
        expect(erreur.message).toMatch(/joindre le serveur/);
    });

    it("traite un proxy sans réponse de l'API (502) comme une erreur réseau", async () => {
        vi.stubGlobal('fetch', vi.fn().mockResolvedValue(reponse(502)));
        const erreur = await appeler('/api/parties/X').catch(e => e);
        expect(erreur.message).toMatch(/joindre le serveur/);
        expect(erreur.reseau).toBe(true);
    });

    it("reste lisible si le serveur ne renvoie pas de JSON", async () => {
        vi.stubGlobal('fetch', vi.fn().mockResolvedValue(reponse(500)));
        const erreur = await appeler('/api/parties/X').catch(e => e);
        expect(erreur.message).toBe('Erreur du serveur (500).');
        expect(erreur.reseau).toBe(true);
    });

    it('accepte une réponse sans contenu (204)', async () => {
        vi.stubGlobal('fetch', vi.fn().mockResolvedValue(reponse(204)));
        await expect(appeler('/api/parties/X/annuler', { methode: 'POST', donnees: {} })).resolves.toBeNull();
    });
});

describe('api', () => {
    it('joue un coup avec le lot conservé et un code encodé', async () => {
        const fetch = vi.fn().mockResolvedValue(reponse(200, {}));
        vi.stubGlobal('fetch', fetch);
        await api.jouer('AB/CD', 'jeton', 0, 3);
        expect(fetch.mock.calls[0][0]).toBe('/api/parties/AB%2FCD/jouer');
        expect(JSON.parse(fetch.mock.calls[0][1].body)).toEqual({ jeton: 'jeton', position: 0, positionConservee: 3 });
    });
});

describe('plusRecente', () => {
    const etat = (code, version) => ({ code, version });

    it("ignore un état diffusé plus ancien que celui affiché", () => {
        expect(plusRecente(etat('A', 3))(etat('A', 5))).toEqual(etat('A', 5));
        expect(plusRecente(etat('A', 6))(etat('A', 5))).toEqual(etat('A', 6));
    });

    it("accepte l'état d'une autre partie", () => {
        expect(plusRecente(etat('B', 1))(etat('A', 9))).toEqual(etat('B', 1));
    });
});
