import { useState } from 'react';
import { usePartie } from './hooks/usePartie.js';
import { ECRANS, ecranDeLaPartie, empreinteFin } from './logique/navigation.js';
import { BandeauConnexion, BandeauErreur } from './components/Bandeaux.jsx';
import EnTete from './components/EnTete.jsx';
import Accueil from './pages/Accueil.jsx';
import Regles from './pages/Regles.jsx';
import Configuration from './pages/Configuration.jsx';
import Pairage from './pages/Pairage.jsx';
import PartieEnCours from './pages/PartieEnCours.jsx';
import BilanManche from './pages/BilanManche.jsx';
import Resultat from './pages/Resultat.jsx';
import TableauHonneur from './pages/TableauHonneur.jsx';

/**
 * Navigation : hors partie, l'écran est choisi par l'utilisateur ; pendant une partie, il découle
 * de l'état reçu du serveur, ce qui garde les deux joueurs synchronisés.
 */
export default function App() {
    const jeu = usePartie();
    const [ecranChoisi, setEcranChoisi] = useState(ECRANS.accueil);
    const [config, setConfig] = useState(null);
    const [finConsultee, setFinConsultee] = useState(null);

    const { partie, session, occupe } = jeu;
    const ecran = ecranDeLaPartie(partie, finConsultee) ?? ecranChoisi;
    const accueil = () => { jeu.quitter(); setEcranChoisi(ECRANS.accueil); };

    function demarrer(nouvelleConfig) {
        setConfig(nouvelleConfig);
        if (nouvelleConfig.mode === 'Local') jeu.creer(nouvelleConfig);
        else setEcranChoisi(ECRANS.pairage);
    }

    let page;
    // Reprise après rafraîchissement : la session est connue mais l'état n'est pas encore relu.
    const chargement = session && !partie;
    switch (chargement ? null : ecran) {
        case null:
            page = <section className="page"><p role="status" className="attente">
                <span className="indicateur" aria-hidden="true" /> Chargement de la partie…</p></section>;
            break;
        case ECRANS.regles:
            page = <Regles retour={() => setEcranChoisi(ECRANS.accueil)} />;
            break;
        case ECRANS.tableauHonneur:
            page = <TableauHonneur retour={() => setEcranChoisi(ECRANS.accueil)} />;
            break;
        case ECRANS.configuration:
            page = <Configuration configInitiale={config} onDemarrer={demarrer} occupe={occupe}
                retour={() => setEcranChoisi(ECRANS.accueil)} />;
            break;
        case ECRANS.pairage:
            page = <Pairage config={config ?? { nom: '', variante: partie?.variante ?? 'Classique' }} partie={partie} occupe={occupe}
                onCreer={() => jeu.creer({ ...config, mode: 'EnLigne', nomJoueur2: null })}
                onRejoindre={code => jeu.rejoindre(code, config.nom)}
                onAnnuler={jeu.annuler}
                retour={() => setEcranChoisi(ECRANS.configuration)} />;
            break;
        case ECRANS.partie:
            page = <PartieEnCours partie={partie} session={session} occupe={occupe} onJouer={jeu.jouer} onQuitter={accueil} />;
            break;
        case ECRANS.bilan:
            page = <BilanManche partie={partie} session={session} occupe={occupe} onContinuer={jeu.continuer}
                onDeclarerNulle={jeu.declarerNulle} onVoirResultat={() => setFinConsultee(empreinteFin(partie))} />;
            break;
        case ECRANS.resultat:
            page = <Resultat partie={partie} session={session} occupe={occupe} onRejouer={jeu.rejouer} onAccueil={accueil} />;
            break;
        default:
            page = <Accueil naviguer={setEcranChoisi} />;
    }

    // Pendant une partie, la marque ne ramène pas à l'accueil : on quitte par « Quitter » (avec confirmation).
    const versAccueil = session ? null : () => setEcranChoisi(ECRANS.accueil);

    return (
        <>
            <a className="lien-evitement" href="#contenu">Aller au contenu</a>
            <EnTete onAccueil={ecran === ECRANS.accueil ? null : versAccueil} connexion={session ? jeu.connexion : null} />
            <main id="contenu" className={`ecran-${ecran ?? 'chargement'}`}>
                {session && <BandeauConnexion connexion={jeu.connexion} onRecharger={jeu.recharger} occupe={occupe} />}
                {page}
            </main>
            <BandeauErreur erreur={jeu.erreur} occupe={occupe} onFermer={jeu.effacerErreur}
                onRecharger={session ? jeu.recharger : undefined} />
            <footer>Kay · jeu de semailles haïtien · INF1011</footer>
        </>
    );
}
