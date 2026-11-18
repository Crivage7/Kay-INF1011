import { useEffect, useState } from 'react';
import { api } from '../api/client.js';
import Icone from '../components/Icone.jsx';
import { initiales } from '../logique/affichage.js';
import { LIBELLES_VARIANTE } from '../logique/navigation.js';

const formatDate = iso => new Date(iso).toLocaleDateString('fr-CA', { year: 'numeric', month: 'long', day: 'numeric' });
const MEDAILLES = ['or', 'argent', 'bronze'];

/** Classement par victoires et meilleurs résultats, persistés côté serveur (devis, sections 6 et 7). */
export default function TableauHonneur({ retour }) {
    const [tableau, setTableau] = useState(null);
    const [erreur, setErreur] = useState('');

    useEffect(() => {
        let ignore = false;
        api.tableauHonneur()
            .then(t => { if (!ignore) setTableau(t); })
            .catch(e => { if (!ignore) setErreur(e.message); });
        return () => { ignore = true; };
    }, []);

    const podium = tableau?.classement.slice(0, 3) ?? [];

    return (
        <section className="page tableau-honneur">
            <button type="button" className="lien-retour" onClick={retour}><Icone nom="retour" taille={18} />Accueil</button>
            <h1>Tableau d'honneur</h1>
            {erreur && <p role="alert" className="bandeau erreur">{erreur}</p>}
            {!tableau && !erreur && <p className="attente" role="status"><span className="indicateur" aria-hidden="true" />Chargement…</p>}

            {tableau && tableau.classement.length === 0 && (
                <div className="panneau vide">
                    <Icone nom="trophee" taille={36} />
                    <p>Aucune partie terminée pour l'instant.<br />Jouez une partie pour inscrire votre nom ici !</p>
                </div>
            )}

            {podium.length > 0 && (
                <ol className="podium" aria-label="Trois premiers du classement">
                    {podium.map((j, i) => (
                        <li key={j.nom} className={`podium-place ${MEDAILLES[i]}`}>
                            <span className="podium-rang">{i + 1}</span>
                            <span className="avatar" aria-hidden="true">{initiales(j.nom)}</span>
                            <strong>{j.nom}</strong>
                            <small>{j.victoires} victoire{j.victoires > 1 ? 's' : ''}</small>
                        </li>
                    ))}
                </ol>
            )}

            {tableau && tableau.classement.length > 0 && <>
                <h2>Classement des joueurs</h2>
                <div className="panneau defilant"><table>
                    <thead><tr>
                        <th scope="col">Rang</th><th scope="col">Joueur</th><th scope="col" className="nombre">Victoires</th>
                        <th scope="col" className="nombre">Nulles</th><th scope="col" className="nombre">Défaites</th><th scope="col" className="nombre">Parties</th>
                    </tr></thead>
                    <tbody>
                        {tableau.classement.map((j, i) => (
                            <tr key={j.nom}><td>{i + 1}</td><th scope="row">{j.nom}</th><td className="nombre">{j.victoires}</td>
                                <td className="nombre">{j.nulles}</td><td className="nombre">{j.defaites}</td><td className="nombre">{j.parties}</td></tr>
                        ))}
                    </tbody>
                </table></div>

                <h2>Meilleurs résultats</h2>
                {tableau.meilleursResultats.length === 0
                    ? <p className="discret">Aucune victoire enregistrée.</p>
                    : <div className="panneau defilant"><table>
                        <thead><tr>
                            <th scope="col">Vainqueur</th><th scope="col">Contre</th><th scope="col" className="nombre">Graines</th>
                            <th scope="col" className="nombre">Manches</th><th scope="col">Variante</th><th scope="col">Date</th>
                        </tr></thead>
                        <tbody>
                            {tableau.meilleursResultats.map(r => (
                                <tr key={r.id}><th scope="row">{r.vainqueur}</th><td>{r.perdant}</td><td className="nombre">{r.grainesVainqueur}</td>
                                    <td className="nombre">{r.manches}</td><td>{LIBELLES_VARIANTE[r.variante]}</td><td>{formatDate(r.termineeLe)}</td></tr>
                            ))}
                        </tbody>
                    </table></div>}
            </>}
        </section>
    );
}
