import Icone from '../components/Icone.jsx';
import { ECRANS } from '../logique/navigation.js';
import { positionsGraines } from '../logique/affichage.js';

/** Petit plateau décoratif : l'état initial du jeu (12 trous de 4 graines). */
function PlateauDecoratif() {
    const trou = i => (
        <span key={i} className="trou deco">
            <span className="graines">{positionsGraines(4).map(({ x, y }, j) => <i key={j} style={{ left: `${x}%`, top: `${y}%` }} />)}</span>
        </span>
    );
    return (
        <div className="plateau plateau-deco" aria-hidden="true">
            <span className="receptacle joueur-2" />
            <div className="plateau-trous">
                <div className="rangee joueur-2">{[0, 1, 2, 3, 4, 5].map(trou)}</div>
                <div className="rangee joueur-1">{[6, 7, 8, 9, 10, 11].map(trou)}</div>
            </div>
            <span className="receptacle joueur-1" />
        </div>
    );
}

export default function Accueil({ naviguer }) {
    return (
        <section className="page accueil">
            <div className="accueil-hero">
                <p className="surtitre">Jeu de semailles haïtien</p>
                <h1>Kay</h1>
                <p className="accroche">Semez, capturez, coupez la tête de votre adversaire. À deux, en ligne ou sur le même écran.</p>
                <button type="button" className="bouton principal grand" onClick={() => naviguer(ECRANS.configuration)}>
                    <Icone nom="jouer" />Jouer
                </button>
            </div>
            <PlateauDecoratif />
            <nav className="accueil-liens" aria-label="Autres sections">
                <button type="button" className="carte-lien" onClick={() => naviguer(ECRANS.regles)}>
                    <span className="carte-lien-icone"><Icone nom="livre" /></span>
                    <span><strong>Règles du jeu</strong><small>Semis, captures, manches et victoire</small></span>
                    <Icone nom="fleche" className="carte-lien-fleche" />
                </button>
                <button type="button" className="carte-lien" onClick={() => naviguer(ECRANS.tableauHonneur)}>
                    <span className="carte-lien-icone"><Icone nom="trophee" /></span>
                    <span><strong>Tableau d'honneur</strong><small>Classement et meilleurs résultats</small></span>
                    <Icone nom="fleche" className="carte-lien-fleche" />
                </button>
            </nav>
        </section>
    );
}
