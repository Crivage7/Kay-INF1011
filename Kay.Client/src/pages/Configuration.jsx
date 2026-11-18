import { useState } from 'react';
import ChoixCartes from '../components/ChoixCartes.jsx';
import Icone from '../components/Icone.jsx';

const LONGUEUR_MAXIMALE_NOM = 30;
const CLE_NOM = 'kay.nom';

const MODES = [
    { valeur: 'EnLigne', titre: 'En ligne', description: 'Deux ordinateurs, pairage par code', icone: 'reseau' },
    { valeur: 'Local', titre: 'Même écran', description: 'Deux joueurs, un seul appareil', icone: 'ecran' },
];

const VARIANTES = [
    { valeur: 'Classique', titre: 'Classique', description: 'Défaite à trois trous désactivés', icone: 'couronne' },
    { valeur: 'CouperLaTete', titre: 'Couper la tête', description: "Jusqu'à ne plus pouvoir remplir aucun trou", icone: 'trophee' },
];

// Le dernier nom saisi est proposé à la visite suivante (commodité propre à ce navigateur).
function nomMemorise() {
    try { return localStorage.getItem(CLE_NOM) ?? ''; } catch { return ''; }
}

/** Saisie des noms, du mode et de la variante (devis, section 6). */
export default function Configuration({ configInitiale, onDemarrer, retour, occupe }) {
    const [nom, setNom] = useState(configInitiale?.nom ?? nomMemorise);
    const [mode, setMode] = useState(configInitiale?.mode ?? 'EnLigne');
    const [nomJoueur2, setNomJoueur2] = useState(configInitiale?.nomJoueur2 ?? '');
    const [variante, setVariante] = useState(configInitiale?.variante ?? 'Classique');
    const [tentative, setTentative] = useState(false);

    const local = mode === 'Local';
    const erreurNom = nom.trim() === '' ? 'Indiquez un nom pour continuer.' : '';
    const erreurNom2 = local && nomJoueur2.trim() === '' ? 'Indiquez le nom du deuxième joueur.' : '';

    function demarrer(evenement) {
        evenement.preventDefault();
        setTentative(true);
        if (erreurNom || erreurNom2) return;
        try { localStorage.setItem(CLE_NOM, nom.trim()); } catch { /* stockage indisponible */ }
        onDemarrer({ nom: nom.trim(), mode, variante, nomJoueur2: local ? nomJoueur2.trim() : null });
    }

    const champ = (id, libelle, valeur, setValeur, erreur, autoFocus) => (
        <div className="champ">
            <label htmlFor={id}>{libelle}</label>
            <input id={id} value={valeur} onChange={e => setValeur(e.target.value)} maxLength={LONGUEUR_MAXIMALE_NOM}
                autoFocus={autoFocus} autoComplete="nickname" aria-invalid={tentative && erreur ? 'true' : undefined}
                aria-describedby={tentative && erreur ? `${id}-erreur` : undefined} />
            {tentative && erreur && <small id={`${id}-erreur`} className="champ-erreur">{erreur}</small>}
        </div>
    );

    return (
        <form className="page configuration" onSubmit={demarrer} noValidate>
            <button type="button" className="lien-retour" onClick={retour}><Icone nom="retour" taille={18} />Accueil</button>
            <h1>Nouvelle partie</h1>

            <ChoixCartes nom="mode" legende="Comment jouez-vous ?" options={MODES} valeur={mode} onChange={setMode} />

            <div className={`champs${local ? ' deux' : ''}`}>
                {champ('nom-joueur-1', local ? 'Nom du joueur 1' : 'Votre nom', nom, setNom, erreurNom, !nom)}
                {local && champ('nom-joueur-2', 'Nom du joueur 2', nomJoueur2, setNomJoueur2, erreurNom2, false)}
            </div>

            <ChoixCartes nom="variante" legende="Fin de partie" options={VARIANTES} valeur={variante} onChange={setVariante} />
            {!local && <p className="discret">En rejoignant la partie d'un autre joueur, vous adoptez sa variante.</p>}

            <div className="actions">
                <button type="button" className="bouton" onClick={retour}>Retour</button>
                <button type="submit" className="bouton principal" disabled={occupe}>
                    {local ? 'Démarrer la partie' : 'Continuer vers le pairage'}<Icone nom="fleche" />
                </button>
            </div>
        </form>
    );
}
