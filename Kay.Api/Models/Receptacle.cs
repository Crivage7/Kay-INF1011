namespace Kay.Api.Models
{
    public class Receptacle
    {
        public int NombreGraines { get; private set; }
        public void AjouterGraines(int quantite)
        {
            if (quantite < 0) throw new ArgumentOutOfRangeException(nameof(quantite));
            checked { NombreGraines += quantite; }
        }
        public int RetirerGraines(int quantite)
        {
            if (quantite < 0 || quantite > NombreGraines)
                throw new ArgumentOutOfRangeException(nameof(quantite));
            NombreGraines -= quantite;
            return quantite;
        }
    }
}
