using HomeServerPage.Data.Astronomy.Telescopes;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Astronomy;

public class AstronomyDbContext : DbContext
{

    public AstronomyDbContext(DbContextOptions<AstronomyDbContext> options) : base(options)
    {
        
    }

    public DbSet<TelescopeItem> Telescopes { get; set; }

    public DbSet<TelescopeEyepiece> Eyepieces { get; set; }

}
