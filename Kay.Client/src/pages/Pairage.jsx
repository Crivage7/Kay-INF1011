import { useEffect, useState } from 'react';
import { api } from '../api/client.js';
import Icone from '../components/Icone.jsx';
import { initiales, tempsEcoule } from '../logique/affichage.js';
import { LIBELLES_VARIANTE } from '../logique/navigation.js';

const INTERVALLE_RAFRAICHISSEMENT_MS = 3000;
const LONGUEUR_CODE = 8;

/** Salle d'attente du créateur : code à partager, en attendant que l'adversaire rejoigne. */
function SalleAttente({ partie, onAnnuler, occupe }) {
    const [copie, setCopie] = useState(null);
    async function copier() {
        try {
            await navigator.clipboard.writeText(partie.code);
            setCopie('ok');
        } catch {
            setCopie('echec');
        }
    }
    return (
        <section className="page salle-attente">
            <div className="attente-entete">
                <span className="pulsation" aria-hidden="true" />
                <h1>En attente de votre adversaire</h1>
                <p className="accroche">La partie commence automatiquement dès qu'il vous rejoint.</p>
            </div>

            <div className="panneau code-panneau">
                <p className="etiquette" id="etiquette-code">Code de la partie</p>
                <p className="code-partie" aria-labelledby="etiquette-code">
                    {[...partie.code].map((c, i) => <span key={i}>{c}</span>)}
                </p>
                <button type="button" className={`bouton${copie === 'ok' ? ' succes' : ''}`} onClick={copier}>
                    <Icone nom={copie === 'ok' ? 'valide' : 'copier'} />{copie === 'ok' ? 'Code copié' : 'Copier le code'}
                </button>
                <p role="status" className="discret">
                    {copie === 'echec' ? 'Copie impossible ici : sélectionnez le code et copiez-le à la main.' : ''}
                </p>
            </div>

            <ol className="etapes">
                <li>Communiquez ce code à votre adversaire.</li>
                <li>Il choisit <strong>Jouer → En ligne</strong>, puis <strong>Rejoindre avec un code</strong>, ou sélectionne votre partie dans la liste.</li>
                <li>La partie démarre ; le premier joueur est tiré au sort.</li>
            </ol>

            <p className="discret centre">
                <span className="puce">{LIBELLES_VARIANTE[partie.variante]}</span>
            </p>

            <div className="actions centre">
                <button type="button" className="bouton discret" onClick={onAnnuler} disabled={occupe}>Annuler la partie</button>
            </div>
        </section>
    );
}

/** Pairage en ligne (exigence du professeur) : créer une partie ou en rejoindre une. */
export default function Pairage({ config, partie, occupe, onCreer, onRejoindre, onAnnuler, retour }) {
    const [code, setCode] = useState('');
    const [enAttente, setEnAttente] = useState([]);
    const [erreurListe, setErreurListe] = useState('');
    const [maintenant, setMaintenant] = useState(() => Date.now());
    const enSalle = partie?.etat === 'EnAttente';

    // Liste des parties en attente, rafraîchie régulièrement.
    useEffect(() => {
        if (enSalle) return;
        let ignore = false;
        const charger = () => api.partiesEnAttente()
            .then(liste => { if (!ignore) { setEnAttente(liste); setErreurListe(''); setMaintenant(Date.now()); } })
            .catch(e => { if (!ignore) setErreurListe(e.message); });
        charger();
        const minuterie = setInterval(charger, INTERVALLE_RAFRAICHISSEMENT_MS);
        return () => { ignore = true; clearInterval(minuterie); };
    }, [enSalle]);

    if (enSalle) return <SalleAttente partie={partie} onAnnuler={onAnnuler} occupe={occupe} />;

    const codeValide = code.trim().length === LONGUEUR_CODE;
    function rejoindreParCode(evenement) {
        evenement.preventDefault();
        if (codeValide) onRejoindre(code.trim());
    }

    return (
        <section className="page pairage">
            <button type="button" className="lien-retour" onClick={retour}><Icone nom="retour" taille={18} />Configuration</button>
            <h1>Trouver un adversaire</h1>
            <p className="identite">
                <span className="avatar petit joueur-1" aria-hidden="true">{initiales(config.nom)}</span>
                Vous jouez sous le nom <strong>{config.nom}</strong>
            </p>

            <div className="pairage-grille">
                <div className="panneau action-pairage">
                    <span className="action-pairage-icone"><Icone nom="joueurs" taille={24} /></span>
                    <h2>Créer une partie</h2>
                    <p>Vous recevrez un code à partager. <span className="puce">{LIBELLES_VARIANTE[config.variante]}</span></p>
                    <button type="button" className="bouton principal" onClick={onCreer} disabled={occupe}>Créer une partie</button>
                </div>

                <form className="panneau action-pairage" onSubmit={rejoindreParCode}>
                    <span className="action-pairage-icone"><Icone nom="lien" taille={24} /></span>
                    <h2>Rejoindre avec un code</h2>
                    <label htmlFor="code-partie" className="visuellement-cache">Code de la partie</label>
                    <input id="code-partie" className="saisie-code" value={code}
                        onChange={e => setCode(e.target.value.toUpperCase().replace(/[^0-9A-F]/g, ''))}
                        placeholder="Ex. 3FA9C21B" maxLength={LONGUEUR_CODE} autoComplete="off" spellCheck={false}
                        inputMode="text" aria-describedby="aide-code" />
                    <small id="aide-code" className="discret">8 caractères, chiffres et lettres de A à F.</small>
                    <button type="submit" className="bouton" disabled={occupe || !codeValide}>Rejoindre</button>
                </form>
            </div>

            <div className="panneau">
                <div className="titre-liste">
                    <h2>Parties en attente</h2>
                    <span className="direct" title="Mise à jour automatique"><span className="point" aria-hidden="true" />En direct</span>
                </div>
                {erreurListe && <p className="discret">{erreurListe}</p>}
                {enAttente.length === 0
                    ? <div className="vide">
                        <Icone nom="joueurs" taille={32} />
                        <p>Aucune partie en attente pour l'instant.<br />Créez-en une et partagez le code !</p>
                    </div>
                    : <ul className="liste-parties">
                        {enAttente.map(p => (
                            <li key={p.code}>
                                <span className="avatar petit joueur-2" aria-hidden="true">{initiales(p.nomCreateur)}</span>
                                <span className="liste-parties-texte">
                                    <strong>{p.nomCreateur}</strong>
                                    <small>{LIBELLES_VARIANTE[p.variante]} · {tempsEcoule(p.creeeLe, maintenant)} · <code>{p.code}</code></small>
                                </span>
                                <button type="button" className="bouton petit" onClick={() => onRejoindre(p.code)} disabled={occupe}
                                    aria-label={`Rejoindre la partie de ${p.nomCreateur}`}>Rejoindre</button>
                            </li>
                        ))}
                    </ul>}
            </div>
        </section>
    );
}
