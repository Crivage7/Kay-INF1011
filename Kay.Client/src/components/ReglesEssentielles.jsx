/** Rappel des règles en quatre points, affiché pendant la partie (aide contextuelle). */
export default function ReglesEssentielles() {
    return (
        <ol className="regles-essentielles">
            <li><strong>Semer.</strong> Prenez toutes les graines d'un de vos trous et déposez-les une à une dans les trous suivants, dans le sens indiqué par les flèches.</li>
            <li><strong>Continuer ou mourir.</strong> Dernière graine dans un trou vide : votre tour s'arrête. Dans un trou garni : reprenez son contenu et continuez.</li>
            <li><strong>Capturer.</strong> Si votre dernière graine fait passer un trou de 3 à 4, vous capturez ces graines (et les trous précédents à 4), puis vous rejouez.</li>
            <li><strong>Gagner.</strong> Quand une rangée est vide, la manche se termine et chacun remplit ses trous à 4 graines. Les trous non remplis sont désactivés ; trois trous désactivés, et la partie est perdue (variante classique).</li>
        </ol>
    );
}
