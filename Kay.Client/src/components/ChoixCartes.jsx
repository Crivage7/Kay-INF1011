import Icone from './Icone.jsx';

/** Groupe de boutons radio présentés en cartes : toute la carte est cliquable, le clavier reste natif. */
export default function ChoixCartes({ nom, legende, options, valeur, onChange }) {
    return (
        <fieldset className="choix-cartes">
            <legend>{legende}</legend>
            <div className="choix-cartes-grille">
                {options.map(option => (
                    <label key={option.valeur} className={`carte-choix${valeur === option.valeur ? ' choisie' : ''}`}>
                        <input type="radio" className="visuellement-cache" name={nom} value={option.valeur}
                            checked={valeur === option.valeur} onChange={() => onChange(option.valeur)} />
                        {option.icone && <span className="carte-choix-icone"><Icone nom={option.icone} taille={22} /></span>}
                        <span className="carte-choix-texte">
                            <strong>{option.titre}</strong>
                            <small>{option.description}</small>
                        </span>
                        <span className="carte-choix-coche" aria-hidden="true"><Icone nom="valide" taille={16} /></span>
                    </label>
                ))}
            </div>
        </fieldset>
    );
}
