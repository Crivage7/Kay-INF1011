// Mémorise la participation à une partie (code, jeton, joueur) dans sessionStorage pour
// survivre à un rafraîchissement de la page. Le stockage peut être indisponible (navigation privée) :
// le jeu fonctionne alors sans reprise.

const CLE = 'kay.session';

export function chargerSession(stockage = globalThis.sessionStorage) {
    try {
        const session = JSON.parse(stockage?.getItem(CLE) ?? 'null');
        return session?.code && session?.jeton && session?.joueurId ? session : null;
    } catch {
        return null;
    }
}

export function enregistrerSession(session, stockage = globalThis.sessionStorage) {
    try { stockage?.setItem(CLE, JSON.stringify(session)); } catch { /* stockage indisponible */ }
}

export function effacerSession(stockage = globalThis.sessionStorage) {
    try { stockage?.removeItem(CLE); } catch { /* stockage indisponible */ }
}
