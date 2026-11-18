using Kay.Api.Models;

namespace Kay.Api.DTOs
{
    public record TableauHonneurDto(
        IReadOnlyList<LigneClassementDto> Classement,
        IReadOnlyList<MeilleurResultatDto> MeilleursResultats);

    public record LigneClassementDto(string Nom, int Victoires, int Nulles, int Defaites, int Parties);

    public record MeilleurResultatDto(
        int Id,
        string Vainqueur,
        string Perdant,
        int GrainesVainqueur,
        int Manches,
        VarianteRegles Variante,
        DateTime TermineeLe);
}
