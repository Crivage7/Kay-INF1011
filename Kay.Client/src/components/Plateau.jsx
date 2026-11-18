import { dispositionPlateau, numeroDuTrou } from '../logique/plateau.js';
import { positionsGraines, sensDuSemis } from '../logique/affichage.js';
import { nomJoueur } from '../logique/navigation.js';

function Graines({ nombre, maximum }) {
    return (
        <span className="graines" aria-hidden="true">
            {positionsGraines(nombre, maximum).map(({ x, y }, i) => <i key={i} style={{ left: `${x}%`, top: `${y}%` }} />)}
        </span>
    );
}

function Trou({ trou, jouable, modifie, version, marque, raccourci, onChoisir }) {
    const numero = numeroDuTrou(trou.position);
    const libelle = !trou.estActive
        ? `Trou ${numero}, désactivé`
        : `Trou ${numero}, ${trou.nombreGraines} graine${trou.nombreGraines > 1 ? 's' : ''}${jouable ? ', jouable' : ''}`;
    const classes = ['trou', trou.estActive ? '' : 'desactive', jouable ? 'jouable' : '', modifie ? 'modifie' : '', marque ? 'selectionne' : '']
        .filter(Boolean).join(' ');
    return (
        // La clé change avec la version quand le trou est modifié : l'animation est rejouée à chaque coup.
        <button key={modifie ? version : 'stable'} type="button" className={classes} disabled={!jouable}
            onClick={() => onChoisir(trou.position)} aria-label={libelle} title={libelle}>
            {trou.estActive && <Graines nombre={trou.nombreGraines} maximum={14} />}
            <span className="trou-compte">{trou.estActive ? trou.nombreGraines : ''}</span>
            {marque && <span className="trou-marque">{marque}</span>}
            {raccourci && <kbd className="trou-raccourci" aria-hidden="true">{raccourci}</kbd>}
        </button>
    );
}

function Receptacle({ partie, joueurId, enHausse }) {
    const nombre = partie.receptacles[joueurId - 1];
    return (
        <div className={`receptacle joueur-${joueurId}${enHausse ? ' en-hausse' : ''}`}
            aria-label={`Réceptacle de ${nomJoueur(partie, joueurId)} : ${nombre} graine${nombre > 1 ? 's' : ''}`} role="img">
            <Graines nombre={nombre} maximum={24} />
            <span className="receptacle-compte">{nombre}</span>
        </div>
    );
}

function Sens({ direction, joueurActifId }) {
    return <div className={`sens sens-${direction} joueur-${joueurActifId}`} aria-hidden="true"><span /></div>;
}

/**
 * Plateau en bois : la rangée du joueur `vuPar` en bas et son réceptacle à droite.
 * Les trous modifiés par le dernier coup sont animés ; le sens du semis du joueur actif est indiqué.
 */
export default function Plateau({ partie, vuPar, estJouable, onChoisir, marques = {}, modifies = new Set(),
    receptaclesEnHausse = [false, false], raccourcis = {} }) {
    const { haut, bas } = dispositionPlateau(vuPar);
    const adversaire = vuPar === 1 ? 2 : 1;
    const sens = sensDuSemis(partie.joueurActifId);
    const enCours = partie.etat === 'EnCours';
    const rangee = (positions, joueurId) => (
        <div className={`rangee joueur-${joueurId}`} role="group" aria-label={`Rangée de ${nomJoueur(partie, joueurId)}`}>
            {positions.map(p => (
                <Trou key={p} trou={partie.cases[p]} jouable={estJouable(p)} modifie={modifies.has(p)} version={partie.version}
                    marque={marques[p]} raccourci={raccourcis[p]} onChoisir={onChoisir} />
            ))}
        </div>
    );
    return (
        <div className="plateau">
            <Receptacle partie={partie} joueurId={adversaire} enHausse={receptaclesEnHausse[adversaire - 1]} />
            <div className="plateau-trous">
                {enCours && <Sens direction={sens.haut} joueurActifId={partie.joueurActifId} />}
                {rangee(haut, adversaire)}
                {rangee(bas, vuPar)}
                {enCours && <Sens direction={sens.bas} joueurActifId={partie.joueurActifId} />}
            </div>
            <Receptacle partie={partie} joueurId={vuPar} enHausse={receptaclesEnHausse[vuPar - 1]} />
        </div>
    );
}
