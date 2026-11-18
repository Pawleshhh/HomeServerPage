using HomeServerPage.Data.Locations;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Locations;

public sealed class LocationsDbContext(DbContextOptions<LocationsDbContext> options) : DbContext(options)
{
    public DbSet<SavedLocation> Locations => this.Set<SavedLocation>();

    public DbSet<LocationSelection> Selections => this.Set<LocationSelection>();

    public DbSet<LocationDatabaseSeedState> SeedStates => this.Set<LocationDatabaseSeedState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var location = modelBuilder.Entity<SavedLocation>();
        location.Property(item => item.Name)
            .IsRequired()
            .HasMaxLength(100);
        location.Property(item => item.TimeZoneId)
            .IsRequired()
            .HasMaxLength(100);
        location.HasIndex(item => item.LegacyObservationPointId)
            .IsUnique();
        location.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_Locations_Latitude",
                "Latitude >= -90 AND Latitude <= 90");
            table.HasCheckConstraint(
                "CK_Locations_Longitude",
                "Longitude >= -180 AND Longitude <= 180");
        });

        var selection = modelBuilder.Entity<LocationSelection>();
        selection.Property(item => item.Feature)
            .HasConversion<string>()
            .HasMaxLength(32);
        selection.HasKey(item => item.Feature);
        selection.HasOne<SavedLocation>()
            .WithMany()
            .HasForeignKey(item => item.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LocationDatabaseSeedState>()
            .HasKey(item => item.Id);
    }
}
