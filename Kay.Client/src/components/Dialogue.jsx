import { useEffect, useId, useRef } from 'react';
import Icone from './Icone.jsx';

/**
 * Boîte de dialogue modale native (<dialog>) : focus piégé, fermeture par Échap,
 * arrière-plan inerte. Contrôlée par `ouvert` ; `onFermer` est appelé à chaque fermeture.
 */
export default function Dialogue({ ouvert, titre, onFermer, children, actions }) {
    const reference = useRef(null);
    const idTitre = useId();

    useEffect(() => {
        const dialogue = reference.current;
        if (!dialogue) return;
        if (ouvert && !dialogue.open) dialogue.showModal();
        if (!ouvert && dialogue.open) dialogue.close();
    }, [ouvert]);

    return (
        <dialog ref={reference} className="dialogue" aria-labelledby={idTitre} onClose={onFermer}
            onClick={e => { if (e.target === reference.current) onFermer(); }}>
            <div className="dialogue-contenu">
                <div className="dialogue-titre">
                    <h2 id={idTitre}>{titre}</h2>
                    <button type="button" className="bouton-icone" onClick={onFermer} aria-label="Fermer"><Icone nom="fermer" /></button>
                </div>
                {children}
                {actions && <div className="actions">{actions}</div>}
            </div>
        </dialog>
    );
}
