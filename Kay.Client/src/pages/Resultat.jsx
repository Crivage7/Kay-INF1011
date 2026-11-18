import Icone from '../components/Icone.jsx';
import { initiales } from '../logique/affichage.js';
import { nomJoueur } from '../logique/navigation.js';

/** Résultat final : vainqueur ou nulle, résumé statistique, rejouer ou revenir à l'accueil. */
export default function Resultat({ partie, session, occupe, onRejouer, onAccueil }) {
    const local = partie.mode === 'Local';
    const jaiDemande = !local && partie.confirmations.includes(session.joueurId);
    const adversaireDemande = !local && partie.confirmations.some(id => id !== session.joueurId);
    const stocks = partie.dernierBilan?.stocks ?? partie.receptacles;
    const victoirePersonnelle = !local && partie.vainqueurId === session.joueurId;
    const titre = partie.estNulle ? 'Partie nulle'
        : victoirePersonnelle ? 'Victoire !'
            : local ? `${nomJoueur(partie, partie.vainqueurId)} gagne !` : 'Défaite';
    const sousTitre = partie.estNulle ? 'Les deux joueurs se séparent à égalité.'
        : victoirePersonnelle || local ? `${nomJoueur(partie, partie.vainqueurId)} l'emporte en ${partie.numeroManche} manche${partie.numeroManche > 1 ? 's' : ''}.`
            : `${nomJoueur(partie, partie.vainqueurId)} l'emporte cette fois. Prenez votre revanche !`;
    const statistiques = [
        ['Graines en fin de partie', stocks],
        ['Graines capturées', partie.grainesCapturees],
        ['Coups joués', partie.coupsJoues],
    ];

    return (
        <section className={`page resultat${partie.estNulle ? ' nulle' : victoirePersonnelle || local ? ' victoire' : ' defaite'}`}>
            <div className="resultat-hero">
                <span className="resultat-embleme"><Icone nom={partie.estNulle ? 'poignee' : 'trophee'} taille={40} /></span>
                <h1>{titre}</h1>
                <p className="accroche">{sousTitre}</p>
            </div>

            <div className="score">
                {[1, 2].map(id => (
                    <div key={id} className={`score-joueur joueur-${id}${partie.vainqueurId === id ? ' gagnant' : ''}`}>
                        <span className="avatar" aria-hidden="true">{initiales(nomJoueur(partie, id))}</span>
                        <strong>{nomJoueur(partie, id)}</strong>
                        <span className="score-valeur">{stocks[id - 1]}</span>
                    </div>
                ))}
            </div>

            <div className="panneau">
                <table className="tableau-stats">
                    <caption className="visuellement-cache">Statistiques de la partie</caption>
                    <thead><tr><th scope="col">Statistique</th><th scope="col">{nomJoueur(partie, 1)}</th><th scope="col">{nomJoueur(partie, 2)}</th></tr></thead>
                    <tbody>
                        {statistiques.map(([libelle, valeurs]) => (
                            <tr key={libelle}><th scope="row">{libelle}</th><td>{valeurs[0]}</td><td>{valeurs[1]}</td></tr>
                        ))}
                        <tr><th scope="row">Manches jouées</th><td colSpan={2} className="commun">{partie.numeroManche}</td></tr>
                    </tbody>
                </table>
            </div>

            {adversaireDemande && !jaiDemande && <p className="encadre centre" role="status"><Icone nom="joueurs" />Votre adversaire propose de rejouer.</p>}

            <div className="actions centre">
                <button type="button" className="bouton" onClick={onAccueil}>Accueil</button>
                <button type="button" className="bouton principal grand" onClick={onRejouer} disabled={occupe || jaiDemande}>
                    {jaiDemande ? <><span className="indicateur" aria-hidden="true" />En attente de l'adversaire…</> : <><Icone nom="actualiser" />Rejouer</>}
                </button>
            </div>
        </section>
    );
}
