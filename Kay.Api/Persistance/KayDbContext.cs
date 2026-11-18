using Kay.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Kay.Api.Persistance
{
    // Configuration base de données SQLite du tableau d'honneur
    public class KayDbContext(DbContextOptions<KayDbContext> options) : DbContext(options)
    {
        public DbSet<ResultatPartie> Resultats => Set<ResultatPartie>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var resultat = modelBuilder.Entity<ResultatPartie>();
            resultat.ToTable("Resultats");
            resultat.HasKey(r => r.Id);
            resultat.Property(r => r.NomJoueur1).HasMaxLength(Joueur.LongueurMaximaleNom).IsRequired();
            resultat.Property(r => r.NomJoueur2).HasMaxLength(Joueur.LongueurMaximaleNom).IsRequired();
            // Énumérations stockées en texte : la base reste lisible et stable si l'ordre des valeurs change.
            resultat.Property(r => r.Variante).HasConversion<string>().HasMaxLength(20);
            resultat.Property(r => r.Mode).HasConversion<string>().HasMaxLength(20);
            // SQLite ne conserve pas le fuseau : les dates sont écrites en UTC et relues comme telles.
            resultat.Property(r => r.TermineeLe)
                .HasConversion(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            resultat.HasIndex(r => r.TermineeLe);
        }
    }
}
