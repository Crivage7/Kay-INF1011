using Kay.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Kay.Api.Middleware
{
    
    // Point unique de traduction des exceptions en réponses HTTP { "erreur" : "…" }.
    // Les contrôleurs n'ont ainsi aucun try/catch et le domaine ignore tout de HTTP.
  
    public sealed class GestionnaireErreurs(ILogger<GestionnaireErreurs> journal) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext contexte, Exception exception, CancellationToken annulation)
        {
            var (statut, message) = exception switch
            {
                PartieIntrouvableException => (StatusCodes.Status404NotFound, exception.Message),
                AccesRefuseException => (StatusCodes.Status403Forbidden, exception.Message),
                ExceptionMetier => (StatusCodes.Status400BadRequest, exception.Message),
                BadHttpRequestException => (StatusCodes.Status400BadRequest, "Requête invalide."),
                _ => (StatusCodes.Status500InternalServerError, "Erreur interne du serveur."),
            };
            if (statut == StatusCodes.Status500InternalServerError)
                journal.LogError(exception, "Erreur non gérée");
            contexte.Response.StatusCode = statut;
            await contexte.Response.WriteAsJsonAsync(new { erreur = message }, annulation);
            return true;
        }
    }
}
