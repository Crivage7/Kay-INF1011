using Kay.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace Kay.Api.Hubs
{
    public class PartieHub : Hub
    {
        // Nom de l'événement diffusé aux clients à chaque changement d'état.
        public const string EvenementPartieModifiee = "PartieModifiee";

        private readonly PartieService _service;
        public PartieHub(PartieService service) => _service = service;

       
         // Abonne la connexion aux mises à jour de la partie. Appelée à la connexion
        // et après chaque reconnexion automatique (les groupes ne survivent pas à une reconnexion).
        
        public async Task Rejoindre(string code, string jeton)
        {
            if (!_service.JetonValide(code, jeton)) throw new HubException("Jeton incorrect ou partie introuvable.");
            await Groups.AddToGroupAsync(Context.ConnectionId, code.ToUpperInvariant());
            await Clients.Caller.SendAsync(EvenementPartieModifiee, _service.Lire(code));
        }
    }
}
