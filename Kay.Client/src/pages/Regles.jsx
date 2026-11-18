import Icone from '../components/Icone.jsx';

const SECTIONS = [
    {
        titre: 'Le plateau',
        contenu: <p>Deux rangées de six trous et un réceptacle par joueur. Chaque trou contient quatre graines au départ, soit 48 graines en tout. Chaque joueur possède la rangée située devant lui.</p>,
    },
    {
        titre: 'Jouer un coup',
        contenu: <ul>
            <li>Le premier joueur est tiré au sort ; les joueurs commencent ensuite les manches à tour de rôle.</li>
            <li>Choisissez un trou actif et non vide de votre rangée : toutes ses graines sont déposées une à une dans les trous suivants. Le premier joueur sème vers sa droite, le second dans le sens contraire ; au bout d'une rangée, on continue dans l'autre. Les trous désactivés sont sautés.</li>
            <li>Dernière graine dans un trou <strong>vide</strong> : vous êtes « mort », votre tour passe.</li>
            <li>Dernière graine dans un trou déjà garni sans capture : vous reprenez son contenu et continuez à semer.</li>
        </ul>,
    },
    {
        titre: 'Le premier tour',
        contenu: <p>Au tout premier coup de la partie, vous pouvez prendre <strong>deux lots</strong> dans deux trous de votre rangée. Le premier est semé ; le second, mis de côté dès le départ, est gardé en réserve. Il n'est semé, à partir de son trou d'origine, que si vous êtes « mort » pendant ce tour. Si vous capturez, vous rejouez et gardez ce lot ; s'il n'a pas servi à la fin de la manche, il rejoint votre réceptacle.</p>,
    },
    {
        titre: 'Les captures',
        contenu: <ul>
            <li>Si votre dernière graine fait passer un trou de trois à <strong>quatre</strong> graines, vous capturez ces quatre graines, que le trou soit chez vous ou chez l'adversaire.</li>
            <li>Les trous précédents qui contiennent aussi exactement quatre graines sont capturés en chaîne (les trous désactivés sont sautés).</li>
            <li>Après une capture, vous rejouez.</li>
            <li>Un coup dont le semis ne se terminerait jamais est refusé : choisissez un autre trou.</li>
        </ul>,
    },
    {
        titre: 'Fin de manche',
        contenu: <ul>
            <li>La manche se termine dès qu'une rangée est vide. L'adversaire ajoute à son réceptacle les graines restées dans sa rangée.</li>
            <li>Chacun remplit ses trous avec les graines de son réceptacle, quatre par trou, dans la limite de six. Les trous non remplis sont désactivés pour la manche suivante, en commençant par celui à votre gauche ; le surplus reste au réceptacle.</li>
            <li>Un bilan de la manche est présenté avant de continuer.</li>
        </ul>,
    },
    {
        titre: 'Victoire, nulle et variantes',
        contenu: <ul>
            <li><strong>Classique :</strong> au moins trois trous désactivés après la redistribution, et la partie est perdue.</li>
            <li><strong>Couper la tête :</strong> on joue jusqu'à ce qu'un joueur ne puisse plus remplir aucun trou ; son adversaire lui a « coupé la tête ».</li>
            <li><strong>Nulle :</strong> un joueur qui avait perdu des graines et revient à 24 (égalité) peut déclarer la partie nulle au bilan, ou continuer.</li>
            <li>Après la partie, vous pouvez rejouer : le plateau est remis à son état initial.</li>
        </ul>,
    },
];

export default function Regles({ retour }) {
    return (
        <section className="page regles">
            <button type="button" className="lien-retour" onClick={retour}><Icone nom="retour" taille={18} />Accueil</button>
            <h1>Règles du Kay</h1>
            <p className="accroche">Le but : capturer assez de graines pour que l'adversaire ne puisse plus remplir ses trous.</p>
            <div className="regles-grille">
                {SECTIONS.map(({ titre, contenu }, i) => (
                    <article key={titre} className="panneau regle">
                        <h2><span className="numero" aria-hidden="true">{i + 1}</span>{titre}</h2>
                        {contenu}
                    </article>
                ))}
            </div>
            <div className="actions">
                <button type="button" className="bouton" onClick={retour}>Retour à l'accueil</button>
            </div>
        </section>
    );
}
