// Accès HTTP à l'API Kay. URL relatives par défaut (proxy Vite en développement) ;
// VITE_API_URL permet de viser une API servie ailleurs.
const BASE = import.meta.env.VITE_API_URL ?? '';

export const URL_HUB = `${BASE}/hubs/partie`;

/** Erreur d'appel : `reseau` vaut vrai quand le serveur n'a pas pu être joint. */
export class ErreurApi extends Error {
    constructor(message, { statut = 0, reseau = false } = {}) {
        super(message);
        this.name = 'ErreurApi';
        this.statut = statut;
        this.reseau = reseau;
    }
}

const MESSAGE_INJOIGNABLE = 'Impossible de joindre le serveur. Vérifiez votre connexion.';
const STATUTS_INJOIGNABLE = [502, 503, 504];

/** Appelle l'API et renvoie le JSON ; lève une ErreurApi avec le message du serveur en cas d'échec. */
export async function appeler(route, { methode = 'GET', donnees } = {}) {
    let reponse;
    try {
        reponse = await fetch(BASE + route, {
            method: methode,
            headers: donnees === undefined ? undefined : { 'Content-Type': 'application/json' },
            body: donnees === undefined ? undefined : JSON.stringify(donnees),
        });
    } catch {
        throw new ErreurApi(MESSAGE_INJOIGNABLE, { reseau: true });
    }
    if (reponse.status === 204) return null;
    const json = await reponse.json().catch(() => null);
    if (!reponse.ok) {
        // 502 à 504 : un intermédiaire (proxy Vite) n'a pas pu joindre l'API.
        if (STATUTS_INJOIGNABLE.includes(reponse.status))
            throw new ErreurApi(MESSAGE_INJOIGNABLE, { statut: reponse.status, reseau: true });
        const reseau = reponse.status >= 500;
        throw new ErreurApi(json?.erreur ?? `Erreur du serveur (${reponse.status}).`, { statut: reponse.status, reseau });
    }
    return json;
}

const partie = code => `/api/parties/${encodeURIComponent(code)}`;
const poster = (route, donnees) => appeler(route, { methode: 'POST', donnees });

export const api = {
    creerPartie: ({ nom, variante, mode, nomJoueur2 }) => poster('/api/parties', { nom, variante, mode, nomJoueur2 }),
    partiesEnAttente: () => appeler('/api/parties/en-attente'),
    rejoindre: (code, nom) => poster(`${partie(code)}/rejoindre`, { nom }),
    annuler: (code, jeton) => poster(`${partie(code)}/annuler`, { jeton }),
    lire: code => appeler(partie(code)),
    jouer: (code, jeton, position, positionConservee = null) => poster(`${partie(code)}/jouer`, { jeton, position, positionConservee }),
    continuer: (code, jeton) => poster(`${partie(code)}/continuer`, { jeton }),
    declarerNulle: (code, jeton) => poster(`${partie(code)}/nulle`, { jeton }),
    rejouer: (code, jeton) => poster(`${partie(code)}/rejouer`, { jeton }),
    tableauHonneur: () => appeler('/api/tableau-honneur'),
};
