namespace Kay.Api.Models
{
    public class Case
    {
        public int Position { get; }
        public int NombreGraines { get; private set; }
        public bool EstActive { get; private set; }
        public Case(int position, int nombreGraines = 4)
        {
            if (position is < 0 or > 11) throw new ArgumentOutOfRangeException(nameof(position));
            if (nombreGraines < 0) throw new ArgumentOutOfRangeException(nameof(nombreGraines));
            Position = position;
            NombreGraines = nombreGraines;
            EstActive = true;
        }
        public void AjouterGraine()
        {
            if (!EstActive) throw new InvalidOperationException("Case désactivée.");
            checked { NombreGraines++; }
        }
        public int RetirerToutesLesGraines()
        {
            if (!EstActive) throw new InvalidOperationException("Case désactivée.");
            int prises = NombreGraines;
            NombreGraines = 0;
            return prises;
        }
        // Désactive un trou vide (aucune graine ne doit disparaître du jeu).
        public void Desactiver()
        {
            if (NombreGraines > 0) throw new InvalidOperationException("Impossible de désactiver un trou qui contient des graines.");
            EstActive = false;
        }
        public void ActiverAvecGraines(int quantite)
        {
            if (quantite < 0) throw new ArgumentOutOfRangeException(nameof(quantite));
            EstActive = true;
            NombreGraines = quantite;
        }
    }
}
