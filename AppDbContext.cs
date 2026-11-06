using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

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
    // Ovu klasu koristi isključivo EF Core alat u VS Code terminalu (dotnet ef migrations add...)
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Izgradnja konfiguracije kako bi fabrika mogla da pročita User Secrets ili appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<AppDbContext>() // Povlači bezbedno kreirane parametre sa vašeg računara
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
            // Povlačenje bezbednog konekcionog niza
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
