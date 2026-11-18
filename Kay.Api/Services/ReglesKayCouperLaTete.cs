using Kay.Api.Models;

namespace Kay.Api.Services
{
   
    // Variante « couper tête ». On joue jusqu'à ce qu'un joueur ne puisse plus remplir aucun trou.
   
    public class ReglesKayCouperLaTete : ReglesKay
    {
        public override VarianteRegles Variante => VarianteRegles.CouperLaTete;

        public override int? DeterminerVainqueur(Plateau plateau) => RangeeGagnante(plateau, Plateau.CasesParRangee);
    }
}
