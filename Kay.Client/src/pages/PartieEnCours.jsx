import { useEffect, useState } from 'react';
import Plateau from '../components/Plateau.jsx';
import Historique from '../components/Historique.jsx';
import CarteJoueur from '../components/CarteJoueur.jsx';
import Dialogue from '../components/Dialogue.jsx';
import Icone from '../components/Icone.jsx';
import ReglesEssentielles from '../components/ReglesEssentielles.jsx';
import { dispositionPlateau, estMonTour, numeroDuTrou, perspective, trouJouable } from '../logique/plateau.js';
import { receptaclesEnHausse, trousModifies } from '../logique/affichage.js';
import { LIBELLES_VARIANTE, nomJoueur } from '../logique/navigation.js';

/** Écran de partie : plateau, joueurs, tour, dernière action ; deux lots au premier tour. */
export default function PartieEnCours({ partie, session, occupe, onJouer, onQuitter }) {
    const [deuxLotsDemandes, setDeuxLotsDemandes] = useState(false);
    const [premierLot, setPremierLot] = useState(null);
    const [dialogue, setDialogue] = useState(null); // 'aide' | 'quitter' | null
    // État précédent, pour animer ce que le dernier coup a changé (motif « état dérivé d'une prop »).
    const [precedente, setPrecedente] = useState(partie);
    const [changements, setChangements] = useState({ trous: new Set(), receptacles: [false, false] });
    if (precedente !== partie) {
        setChangements({ trous: trousModifies(precedente, partie), receptacles: receptaclesEnHausse(precedente, partie) });
        setPrecedente(partie);
    }

    const vuPar = perspective(partie, session);
    const adversaire = vuPar === 1 ? 2 : 1;
    const local = partie.mode === 'Local';
    const monTour = estMonTour(partie, session);
    const deuxLots = deuxLotsDemandes && partie.deuxLotsPermis && monTour;
    const lotChoisi = deuxLots ? premierLot : null;
    const actif = nomJoueur(partie, partie.joueurActifId);
    const jouable = position => !occupe && trouJouable(partie, session, position);
    // Second lot du premier tour gardé en réserve (semé seulement si son joueur est « mort »).
    const lot = partie.lotConserve;
    const lotAMoi = !local && lot?.joueurId === session.joueurId;

    function choisir(position) {
        if (!deuxLots) return onJouer(position);
        if (lotChoisi === null) return setPremierLot(position);
        if (lotChoisi === position) return setPremierLot(null);
        setPremierLot(null);
        setDeuxLotsDemandes(false);
        return onJouer(lotChoisi, position);
    }

    // Raccourcis clavier 1 à 6 : n-ième trou, de gauche à droite, de la rangée du joueur actif.
    const { haut, bas } = dispositionPlateau(vuPar);
    const rangeeActive = monTour ? (partie.joueurActifId === vuPar ? bas : haut) : [];
    const raccourcis = Object.fromEntries(rangeeActive.filter(jouable).map(p => [p, String(rangeeActive.indexOf(p) + 1)]));
    useEffect(() => {
        function touche(evenement) {
            if (dialogue || evenement.ctrlKey || evenement.metaKey || evenement.altKey) return;
            if (evenement.target instanceof HTMLElement && evenement.target.closest('input, textarea')) return;
            const position = Object.keys(raccourcis).find(p => raccourcis[p] === evenement.key);
            if (position !== undefined) { evenement.preventDefault(); choisir(Number(position)); }
        }
        window.addEventListener('keydown', touche);
        return () => window.removeEventListener('keydown', touche);
    });

    const tour = local ? `À ${actif} de jouer` : monTour ? 'À vous de jouer' : `${actif} réfléchit…`;
    const consigne = !monTour
        ? 'Les coups de votre adversaire apparaissent ici en direct.'
        : deuxLots
            ? (lotChoisi === null ? 'Lot 1 : choisissez le trou à semer en premier.' : 'Lot 2 : choisissez le trou à garder en réserve.')
            : <>{local ? `${actif}, choisissez un trou de votre rangée` : 'Choisissez un trou de votre rangée'}<span className="indice-clavier"> (touches 1 à 6)</span>.</>;

    return (
        <section className="page partie">
            <div className="partie-entete">
                <h1>Manche {partie.numeroManche}</h1>
                <div className="puces">
                    <span className="puce">{LIBELLES_VARIANTE[partie.variante]}</span>
                    {local ? <span className="puce">Même écran</span> : <span className="puce">Code <code>{partie.code}</code></span>}
                </div>
                <div className="partie-outils">
                    <button type="button" className="bouton discret petit" onClick={() => setDialogue('aide')}><Icone nom="aide" taille={18} />Aide</button>
                    <button type="button" className="bouton discret petit" onClick={() => setDialogue('quitter')}><Icone nom="quitter" taille={18} />Quitter</button>
                </div>
            </div>

            <CarteJoueur partie={partie} joueurId={adversaire} role={local ? `Joueur ${adversaire}` : 'Adversaire'} />

            <div className={`statut-tour${monTour ? ' a-vous' : ''} joueur-${partie.joueurActifId}`}>
                <p className="tour" role="status" aria-live="polite">{tour}</p>
                <p className="consigne">{consigne}</p>
            </div>

            <Plateau partie={partie} vuPar={vuPar} onChoisir={choisir} estJouable={jouable}
                marques={lotChoisi !== null ? { [lotChoisi]: 'Lot 1' } : {}} modifies={changements.trous}
                receptaclesEnHausse={changements.receptacles} raccourcis={raccourcis} />

            <CarteJoueur partie={partie} joueurId={vuPar} role={local ? `Joueur ${vuPar}` : 'Vous'} estMoi={!local} />

            {partie.deuxLotsPermis && monTour && (
                <label className={`interrupteur${deuxLotsDemandes ? ' actif' : ''}`}>
                    <input type="checkbox" role="switch" checked={deuxLotsDemandes}
                        onChange={e => { setDeuxLotsDemandes(e.target.checked); setPremierLot(null); }} />
                    <span className="interrupteur-piste" aria-hidden="true"><span /></span>
                    <span>
                        <strong>Premier tour : prendre deux lots</strong>
                        <small>Le second lot est gardé en réserve : il sera semé depuis son trou si vous êtes « mort » pendant ce tour.</small>
                    </span>
                </label>
            )}

            {lot && (
                <div className="encadre" role="status">
                    <Icone nom="historique" />
                    <p>
                        {lotAMoi ? 'Vous gardez' : `${nomJoueur(partie, lot.joueurId)} garde`} un lot de <strong>{lot.graines} graine{lot.graines > 1 ? 's' : ''}</strong> en
                        réserve (trou {numeroDuTrou(lot.position)}). Il sera semé depuis ce trou {lotAMoi ? 'si vous êtes' : "si ce joueur est"} « mort » pendant ce tour.
                    </p>
                </div>
            )}

            <p className="derniere-action" aria-live="polite">
                <Icone nom="historique" taille={16} />
                <span>{partie.historique.at(-1) ?? 'La partie commence.'}</span>
            </p>

            <Historique lignes={partie.historique} />

            <Dialogue ouvert={dialogue === 'aide'} titre="Comment jouer" onFermer={() => setDialogue(null)}
                actions={<button type="button" className="bouton principal" onClick={() => setDialogue(null)}>J'ai compris</button>}>
                <ReglesEssentielles />
            </Dialogue>

            <Dialogue ouvert={dialogue === 'quitter'} titre="Quitter la partie ?" onFermer={() => setDialogue(null)}
                actions={<>
                    <button type="button" className="bouton" onClick={() => setDialogue(null)}>Continuer à jouer</button>
                    <button type="button" className="bouton danger" onClick={() => { setDialogue(null); onQuitter(); }}>Quitter</button>
                </>}>
                <p>{local ? 'La partie en cours sera abandonnée.' : "Vous quitterez la partie ; votre adversaire ne pourra plus la terminer avec vous."}</p>
            </Dialogue>
        </section>
    );
}
