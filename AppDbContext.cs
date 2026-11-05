using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UporediPlate
{
    // 1. GLAVNI KONTEKST BAZE PODATAKA
    // Ova klasa se koristi tokom rada aplikacije (Runtime)
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ====================================================================
        // OVDE DODAJETE VAŠE TABELE (MODELE) NAKON ŠTO IH NAPRAVITE
        // Primer:
        // public DbSet<Zaposleni> Zaposleni { get; set; }
        // public DbSet<Plata> Plate { get; set; }
        // ====================================================================
    }

    // 2. FABRIKA ZA KREIRANJE MIGRACIJA (Design-time)
    // Ovu klasu koristi isključivo EF Core alat u VS Code terminalu
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
            // Konfiguracija: SQL Server Autentifikacija za server 10.20.0.203 i bazu UporediPlate
            // Zamenite TVOJ_SQL_USER i TVOJA_SQL_LOZINKA sa vašim stvarnim parametrima
            optionsBuilder.UseSqlServer("Server=10.20.0.203;Database=UporediPlate;User Id=UporediPlate;Password=Pantelej13.;TrustServerCertificate=True;Connect Timeout=15;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
