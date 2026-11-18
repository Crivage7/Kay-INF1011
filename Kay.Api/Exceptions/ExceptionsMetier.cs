namespace Kay.Api.Exceptions
{
   
    // Erreur prévue par le domaine, dont le message peut être montré tel quel au joueur.
    // La traduction en code HTTP est faite par la couche API (GestionnaireErreurs), pas ici.
  
    public abstract class ExceptionMetier(string message) : Exception(message);

    // Le trou choisi ne peut pas être joué (vide, désactivé, hors rangée, pas votre tour…).
    public sealed class CoupInvalideException(string message) : ExceptionMetier(message);

    // L'action n'est pas permise dans l'état actuel de la partie (partie complète, non commencée…).
    public sealed class ActionRefuseeException(string message) : ExceptionMetier(message);

    public sealed class PartieIntrouvableException(string code) : ExceptionMetier($"La partie {code} est introuvable.");

    // Le jeton fourni ne correspond à aucun joueur de la partie.
    public sealed class AccesRefuseException(string message) : ExceptionMetier(message);
}
