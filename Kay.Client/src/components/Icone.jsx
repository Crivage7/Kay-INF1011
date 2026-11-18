// Jeu d'icônes linéaires (24 × 24, trait de 2) : décoratives, toujours accompagnées d'un texte.
const TRACES = {
    jouer: <path d="M7 4.5v15l12-7.5z" />,
    livre: <><path d="M4 5.5A2.5 2.5 0 0 1 6.5 3H20v15H6.5A2.5 2.5 0 0 0 4 20.5z" /><path d="M4 20.5A2.5 2.5 0 0 0 6.5 23H20v-5" /></>,
    trophee: <><path d="M8 4h8v5a4 4 0 0 1-8 0z" /><path d="M8 6H5a3 3 0 0 0 3 4M16 6h3a3 3 0 0 1-3 4" /><path d="M12 13v4M8 21h8M9.5 17h5l.5 4h-6z" /></>,
    retour: <path d="M15 5l-7 7 7 7" />,
    copier: <><rect x="9" y="9" width="11" height="11" rx="2" /><path d="M5 15V6a2 2 0 0 1 2-2h8" /></>,
    valide: <path d="M5 12.5l4.5 4.5L19 7.5" />,
    reseau: <><circle cx="12" cy="12" r="9" /><path d="M3 12h18M12 3a14 14 0 0 1 0 18M12 3a14 14 0 0 0 0 18" /></>,
    ecran: <><rect x="3" y="4" width="18" height="12" rx="2" /><path d="M8 20h8M12 16v4" /></>,
    joueurs: <><circle cx="9" cy="8" r="3.5" /><path d="M2.5 20a6.5 6.5 0 0 1 13 0" /><circle cx="17" cy="9" r="2.5" /><path d="M17 14.5a5 5 0 0 1 4.5 5.5" /></>,
    quitter: <><path d="M14 4h4a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-4" /><path d="M9 8l-4 4 4 4M5 12h11" /></>,
    aide: <><circle cx="12" cy="12" r="9" /><path d="M9.5 9.5a2.5 2.5 0 1 1 3.5 2.3c-.7.3-1 .8-1 1.5v.7" /><path d="M12 17h.01" /></>,
    historique: <><circle cx="12" cy="12" r="9" /><path d="M12 7v5l3 2" /></>,
    poignee: <><path d="M7 11l3-3 3 2 4-4 4 4-8 8-3-3-3 3-4-4z" /></>,
    couronne: <path d="M3 8l4.5 4L12 5l4.5 7L21 8l-2 11H5z" />,
    fleche: <path d="M5 12h14M13 6l6 6-6 6" />,
    lien: <><path d="M10 14a4 4 0 0 0 5.7 0l3-3a4 4 0 0 0-5.7-5.7l-1 1" /><path d="M14 10a4 4 0 0 0-5.7 0l-3 3a4 4 0 0 0 5.7 5.7l1-1" /></>,
    actualiser: <><path d="M20 11a8 8 0 1 0-2.3 5.7" /><path d="M20 4v7h-7" /></>,
    fermer: <path d="M6 6l12 12M18 6L6 18" />,
};

export default function Icone({ nom, taille = 20, className = '' }) {
    return (
        <svg className={`icone ${className}`} width={taille} height={taille} viewBox="0 0 24 24" fill="none"
            stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" focusable="false">
            {TRACES[nom]}
        </svg>
    );
}
