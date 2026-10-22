using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Exercise;

public sealed class ExerciseDbContext(DbContextOptions<ExerciseDbContext> options) : DbContext(options)
{
    public DbSet<ExerciseEntry> ExerciseEntries => Set<ExerciseEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExerciseEntry>(entity =>
        {
            entity.Property(entry => entry.LocalDate).HasColumnType("TEXT");
            entity.Property(entry => entry.LoggedAt).HasColumnType("TEXT");
            entity.Property(entry => entry.DistanceKm).HasColumnType("TEXT");
            entity.HasIndex(entry => entry.LocalDate);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_ExerciseEntries_Calories", "CaloriesBurned > 0");
                table.HasCheckConstraint(
                    "CK_ExerciseEntries_Distance",
                    "DistanceKm IS NULL OR CAST(DistanceKm AS REAL) > 0");
            });
        });
    }
}
