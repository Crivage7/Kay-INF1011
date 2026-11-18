namespace Kay.Api.Models
{
    public enum EtatPartie
    {
        // En attente du deuxième joueur.
        EnAttente,
        EnCours,
        // Manche terminée sans vainqueur : le bilan est affiché jusqu'à ce que les deux joueurs continuent.
        FinDeManche,
        Terminee
    }
}
