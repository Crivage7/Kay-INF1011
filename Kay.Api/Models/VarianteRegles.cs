namespace Kay.Api.Models
{
    public enum VarianteRegles
    {
        // Un joueur perd dès qu'il a au moins trois trous désactivés après redistribution.
        Classique,
        // On joue jusqu'à ce qu'un joueur ne puisse plus remplir aucun trou.
        CouperLaTete
    }
}
