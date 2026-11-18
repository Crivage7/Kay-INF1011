using Kay.Api.DTOs;
using Kay.Api.Hubs;
using Kay.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Kay.Api.Controllers;


/*

    Point d'entrée HTTP : traduit les requêtes en appels au service puis diffuse le nouvel état
    aux joueurs de la partie. Les erreurs sont gérées par GestionnaireErreurs.

*/


[ApiController]
[Route("api/parties")]
public class PartiesController : ControllerBase
{
    private readonly PartieService _service;
    private readonly IHubContext<PartieHub> _hub;
    public PartiesController(PartieService service, IHubContext<PartieHub> hub)
    { _service = service; _hub = hub; }

    [HttpPost]
    public ActionResult<AccesPartieReponse> Creer([FromBody] CreerPartieRequete req) =>
        Ok(_service.Creer(req.Nom, req.Variante, req.Mode, req.NomJoueur2));

    // Parties en ligne qui attendent un adversaire (écran de pairage)
    [HttpGet("en-attente")]
    public ActionResult<IReadOnlyList<PartieEnAttenteDto>> EnAttente() => Ok(_service.PartiesEnAttente());

    // Le créateur annule une partie que personne n'a encore rejointe
    [HttpPost("{code}/annuler")]
    public IActionResult Annuler(string code, [FromBody] JetonRequete req)
    {
        _service.Annuler(code, req.Jeton);
        return NoContent();
    }

    [HttpPost("{code}/rejoindre")]
    public async Task<ActionResult<AccesPartieReponse>> Rejoindre(string code, [FromBody] RejoindrePartieRequete req)
    {
        var resultat = _service.Rejoindre(code, req.Nom);
        await Diffuser(resultat.Partie);
        return Ok(resultat);
    }

    [HttpGet("{code}")]
    public ActionResult<PartieDto> Lire(string code) => Ok(_service.Lire(code));

    [HttpPost("{code}/jouer")]
    public async Task<ActionResult<PartieDto>> Jouer(string code, [FromBody] JouerRequete req)
    {
        var dto = _service.Jouer(code, req.Jeton, req.Position, req.PositionConservee);
        await Diffuser(dto);
        return Ok(dto);
    }

    // Le joueur a lu le bilan de manche ; la manche suivante démarre quand les deux ont continué
    [HttpPost("{code}/continuer")]
    public async Task<ActionResult<PartieDto>> Continuer(string code, [FromBody] JetonRequete req)
    {
        var dto = _service.Continuer(code, req.Jeton);
        await Diffuser(dto);
        return Ok(dto);
    }

    // Le joueur qui a récupéré toutes ses graines déclare la partie nulle (au bilan de manche) 
    [HttpPost("{code}/nulle")]
    public async Task<ActionResult<PartieDto>> DeclarerNulle(string code, [FromBody] JetonRequete req)
    {
        var dto = _service.DeclarerNulle(code, req.Jeton);
        await Diffuser(dto);
        return Ok(dto);
    }

    // Le joueur veut rejouer ; une nouvelle partie démarre quand les deux l'ont demandé
    [HttpPost("{code}/rejouer")]
    public async Task<ActionResult<PartieDto>> Rejouer(string code, [FromBody] JetonRequete req)
    {
        var dto = _service.Rejouer(code, req.Jeton);
        await Diffuser(dto);
        return Ok(dto);
    }

    private Task Diffuser(PartieDto partie) =>
        _hub.Clients.Group(partie.Code).SendAsync(PartieHub.EvenementPartieModifiee, partie);
}
