using Kay.Api.DTOs;
using Kay.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kay.Api.Controllers;

// Tableau d'honneur : classement par victoires et meilleurs résultats
[ApiController]
[Route("api/tableau-honneur")]
public class TableauHonneurController(TableauHonneurService service) : ControllerBase
{
    [HttpGet]
    public ActionResult<TableauHonneurDto> Obtenir() => Ok(service.Obtenir());
}
