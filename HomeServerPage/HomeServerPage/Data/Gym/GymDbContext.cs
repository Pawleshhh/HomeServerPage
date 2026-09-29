using HomeServerPage.Helpers;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Gym;

public class GymDbContext : DbContext
{
    protected GymDbContext(DbContextOptions<GymDbContext> options) : base(options)
    {
    }

    public DbSet<GymSession> Sessions { get; set; }

    public DbSet<GymMeasurement> Measurements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        using var document = ResourcesHelper.GetJsonDocument("exercises");

    }

}
