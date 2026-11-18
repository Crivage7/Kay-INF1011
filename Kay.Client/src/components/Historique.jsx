import Icone from './Icone.jsx';

/** Historique des coups, repliable, le plus récent en premier. */
export default function Historique({ lignes }) {
    return (
        <details className="historique panneau">
            <summary>
                <Icone nom="historique" />
                <span>Historique des coups</span>
                <span className="compteur">{lignes.length}</span>
            </summary>
            {lignes.length === 0
                ? <p className="discret">Aucun coup pour l'instant.</p>
                : <ol reversed>{[...lignes].reverse().map((ligne, i) => <li key={lignes.length - i}>{ligne}</li>)}</ol>}
        </details>
    );
}
