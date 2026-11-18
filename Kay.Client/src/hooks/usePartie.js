import { useCallback, useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { api, URL_HUB } from '../api/client.js';
import { chargerSession, effacerSession, enregistrerSession } from '../logique/session.js';

// Les diffusions SignalR peuvent arriver dans le désordre : on garde l'état le plus récent.
export const plusRecente = nouvelle => actuelle =>
    !actuelle || actuelle.code !== nouvelle.code || nouvelle.version >= actuelle.version ? nouvelle : actuelle;

/**
 * Participation à une partie : session (reprise après rafraîchissement), état reçu du serveur,
 * connexion temps réel, actions de jeu et erreurs. Les pages n'appellent jamais l'API directement.
 */
export function usePartie() {
    const [session, setSession] = useState(chargerSession);
    const [partie, setPartie] = useState(null);
    const [erreur, setErreur] = useState(null);
    const [occupe, setOccupe] = useState(false);
    // 'connexion' (en cours), 'connecte', 'reconnexion' ou 'deconnecte'.
    const [connexion, setConnexion] = useState('connexion');

    const recevoir = useCallback(p => setPartie(plusRecente(p)), []);

    const quitter = useCallback(() => {
        effacerSession();
        setSession(null);
        setPartie(null);
        setConnexion('connexion');
    }, []);

    // Les parties vivent en mémoire côté serveur : après un redémarrage, la session ne mène plus nulle part.
    const partiePerdue = useCallback(() => {
        quitter();
        setErreur({ message: "Cette partie n'existe plus (le serveur a peut-être redémarré).", reseau: false });
    }, [quitter]);

    // Reprise : relit l'état de la partie mémorisée (après un rafraîchissement de la page).
    useEffect(() => {
        if (!session) return;
        let ignore = false;
        api.lire(session.code).then(p => { if (!ignore) recevoir(p); }).catch(e => {
            if (ignore) return;
            if (e.statut === 404) partiePerdue();
            else setErreur({ message: e.message, reseau: true });
        });
        return () => { ignore = true; };
    }, [session, recevoir, partiePerdue]);

    // Temps réel : abonnement aux changements de la partie, rétabli après chaque reconnexion.
    useEffect(() => {
        if (!session) return;
        let ignore = false;
        const hub = new signalR.HubConnectionBuilder()
            .withUrl(URL_HUB)
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Warning)
            .build();
        const abonner = () => hub.invoke('Rejoindre', session.code, session.jeton);
        hub.on('PartieModifiee', p => { if (!ignore) recevoir(p); });
        hub.onreconnecting(() => { if (!ignore) setConnexion('reconnexion'); });
        hub.onreconnected(() => {
            if (ignore) return;
            setConnexion('connecte');
            abonner().catch(e => setErreur({ message: e.message, reseau: true }));
        });
        hub.onclose(() => { if (!ignore) setConnexion('deconnecte'); });
        hub.start()
            .then(abonner)
            .then(() => { if (!ignore) setConnexion('connecte'); })
            .catch(() => {
                if (ignore) return;
                setConnexion('deconnecte');
                setErreur({ message: 'Connexion temps réel impossible : utilisez « Recharger l\'état ».', reseau: true });
            });
        return () => { ignore = true; hub.stop(); };
    }, [session, recevoir]);

    /**
     * Exécute une requête en désactivant les commandes ; les erreurs sont présentées à l'utilisateur.
     * `surPartieCourante` : l'action vise la partie de la session, un 404 signifie donc qu'elle est perdue.
     */
    const executer = useCallback(async (action, surPartieCourante = false) => {
        setErreur(null);
        setOccupe(true);
        try {
            return await action();
        } catch (e) {
            if (surPartieCourante && e.statut === 404) partiePerdue();
            else setErreur({ message: e.message, reseau: Boolean(e.reseau) });
            return null;
        } finally {
            setOccupe(false);
        }
    }, [partiePerdue]);

    const entrer = useCallback(acces => {
        const nouvelle = { code: acces.partie.code, jeton: acces.jeton, joueurId: acces.joueurId };
        enregistrerSession(nouvelle);
        recevoir(acces.partie);
        setConnexion('connexion');
        setSession(nouvelle);
    }, [recevoir]);

    const agir = appel => executer(async () => recevoir(await appel(session.code, session.jeton)), true);

    return {
        session, partie, erreur, occupe, connexion,
        effacerErreur: () => setErreur(null),
        creer: config => executer(async () => entrer(await api.creerPartie(config))),
        rejoindre: (code, nom) => executer(async () => entrer(await api.rejoindre(code.trim(), nom))),
        jouer: (position, positionConservee) =>
            agir((code, jeton) => api.jouer(code, jeton, position, positionConservee)),
        continuer: () => agir(api.continuer),
        declarerNulle: () => agir(api.declarerNulle),
        rejouer: () => agir(api.rejouer),
        recharger: () => executer(async () => recevoir(await api.lire(session.code)), true),
        annuler: () => executer(async () => { await api.annuler(session.code, session.jeton); quitter(); }, true),
        quitter,
    };
}
