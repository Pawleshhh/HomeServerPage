using HomeServerPage.Client.Data.Astronomy.Telescopes;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Astronomy;

public class AstronomyDbContext(DbContextOptions<AstronomyDbContext> options) : DbContext(options)
{
    public DbSet<TelescopeItem> Telescopes { get; set; }

    public DbSet<TelescopeEyepiece> Eyepieces { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TelescopeItem>().HasData(
            new TelescopeItem(
                Id: 1,
                Name: "Sky-Watcher BKMAK 127 OTAW",
                Type: TelescopeType.MaksutovCassegrain,
                Aperture: 127,
                FocalLength: 1500,
                ApertureSpeed: 11.8),
            new TelescopeItem(
                Id: 2,
                Name: "Sky-Watcher Evostar 72ED 72/420 F6",
                Type: TelescopeType.Refractor,
                Aperture: 72,
                FocalLength: 420,
                ApertureSpeed: 5.8),
            new TelescopeItem(
                Id: 2,
                Name: "Sky-Watcher BK 1309 130/900",
                Type: TelescopeType.Newtonian,
                Aperture: 130,
                FocalLength: 900,
                ApertureSpeed: 6.9));

        modelBuilder.Entity<TelescopeEyepiece>().HasData(
            new TelescopeEyepiece(
                Id: 1,
                FocalLength: 25,
                FieldOfView: 52,
                BarrelDiameter: 1.25));
    }
}
