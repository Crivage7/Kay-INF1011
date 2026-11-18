import Icone from '../components/Icone.jsx';
import { initiales } from '../logique/affichage.js';
import { nomJoueur } from '../logique/navigation.js';

const GRAINES_TOTALES = 48;
const GRAINES_PAR_JOUEUR = 24;

/** Bilan d'un joueur : stock, jauge par rapport à l'égalité, trous de la manche suivante. */
function BilanJoueur({ partie, joueurId, bilan }) {
    const i = joueurId - 1;
    const stock = bilan.stocks[i];
    const actifs = bilan.trousActifs[i];
    return (
        <article className={`panneau bilan-joueur joueur-${joueurId}`}>
            <header>
                <span className="avatar" aria-hidden="true">{initiales(nomJoueur(partie, joueurId))}</span>
                <h2>{nomJoueur(partie, joueurId)}</h2>
            </header>
            <p className="bilan-stock"><strong>{stock}</strong> graines</p>
            <div className="jauge" role="img" aria-label={`${stock} graines sur 48 ; l'égalité est à 24`}>
                <span className="jauge-remplissage" style={{ width: `${(stock / GRAINES_TOTALES) * 100}%` }} />
                <span className="jauge-egalite" style={{ left: `${(GRAINES_PAR_JOUEUR / GRAINES_TOTALES) * 100}%` }} />
            </div>
            <div className="mini-trous" role="img" aria-label={`${actifs} trous actifs sur 6 pour la manche suivante`}>
                {Array.from({ length: 6 }, (_, k) => <span key={k} className={k < 6 - actifs ? 'desactive' : ''} />)}
            </div>
            <dl className="bilan-details">
                <div><dt>Ramassées dans la rangée</dt><dd>{bilan.grainesRamassees[i]}</dd></div>
                <div><dt>Trous actifs</dt><dd>{actifs} / 6</dd></div>
                <div><dt>Restées au réceptacle</dt><dd>{bilan.reserves[i]}</dd></div>
            </dl>
        </article>
    );
}

/** Bilan de la manche terminée : graines récupérées et redistribution (devis, sections 3 et 6). */
export default function BilanManche({ partie, session, occupe, onContinuer, onDeclarerNulle, onVoirResultat }) {
    const bilan = partie.dernierBilan;
    const terminee = partie.etat === 'Terminee';
    const local = partie.mode === 'Local';
    const jaiConfirme = !local && partie.confirmations.includes(session.joueurId);
    const adversaireAConfirme = !local && partie.confirmations.some(id => id !== session.joueurId);
    const peutDeclarerNulle = partie.joueurPouvantDeclarerNulle !== null
        && (local || partie.joueurPouvantDeclarerNulle === session.joueurId);

    return (
        <section className="page bilan">
            <p className="surtitre">Fin de la manche {bilan.numeroManche}</p>
            <h1>{terminee ? `${nomJoueur(partie, partie.vainqueurId)} remporte la partie` : 'La partie continue'}</h1>
            <p className="accroche">
                {terminee
                    ? 'Voici la dernière redistribution avant le résultat final.'
                    : `Chacun a rempli ses trous à quatre graines. Prochaine étape : la manche ${partie.numeroManche + 1}.`}
            </p>

            <div className="bilan-grille">
                <BilanJoueur partie={partie} joueurId={1} bilan={bilan} />
                <BilanJoueur partie={partie} joueurId={2} bilan={bilan} />
            </div>

            {partie.joueurPouvantDeclarerNulle !== null && !terminee && (
                <div className="encadre">
                    <Icone nom="poignee" />
                    <p>
                        <strong>{nomJoueur(partie, partie.joueurPouvantDeclarerNulle)}</strong> a récupéré toutes ses graines.
                        {peutDeclarerNulle ? ' Vous pouvez déclarer la partie nulle, ou continuer à jouer.' : ' Il peut déclarer la partie nulle.'}
                    </p>
                </div>
            )}

            {adversaireAConfirme && !jaiConfirme && <p className="discret centre" role="status">Votre adversaire est prêt pour la manche suivante.</p>}

            <div className="actions centre">
                {terminee && <button type="button" className="bouton principal grand" onClick={onVoirResultat}>Voir le résultat<Icone nom="fleche" /></button>}
                {!terminee && peutDeclarerNulle && <button type="button" className="bouton" onClick={onDeclarerNulle} disabled={occupe}>Déclarer la partie nulle</button>}
                {!terminee && (
                    <button type="button" className="bouton principal grand" onClick={onContinuer} disabled={occupe || jaiConfirme}>
                        {jaiConfirme ? <><span className="indicateur" aria-hidden="true" />En attente de l'adversaire…</> : <>Continuer<Icone nom="fleche" /></>}
                    </button>
                )}
            </div>
        </section>
    );
}
