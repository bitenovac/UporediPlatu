using Microsoft.EntityFrameworkCore;
using UporediPlate.Models; // Proverite da li se vaš folder sa modelima zove Models

namespace UporediPlate.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Ovde registrujete tabele koje želite u bazi. 
    // Ako još nemate klasu "Zaposleni", privremeno zakomentarišite red ispod sa //
   // public DbSet<Zaposleni> Zaposleni { get; set; }
}
