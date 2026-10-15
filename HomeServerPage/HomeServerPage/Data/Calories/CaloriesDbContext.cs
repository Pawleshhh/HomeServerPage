using HomeServerPage.Data.Calories;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Calories;

public sealed class CaloriesDbContext(DbContextOptions<CaloriesDbContext> options) : DbContext(options)
{
    public DbSet<CaloriesFood> Foods => Set<CaloriesFood>();

    public DbSet<CaloriesLogEntry> Entries => Set<CaloriesLogEntry>();

    public DbSet<CaloriesTrackerSettings> Settings => Set<CaloriesTrackerSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CaloriesFood>(entity =>
        {
            entity.Property(food => food.Name)
                .IsRequired()
                .HasMaxLength(160);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_CaloriesFoods_Calories", "CaloriesPerServing > 0");
                table.HasCheckConstraint(
                    "CK_CaloriesFoods_Macros",
                    "(CarbohydratesGrams IS NULL OR CarbohydratesGrams >= 0) " +
                    "AND (ProteinGrams IS NULL OR ProteinGrams >= 0) " +
                    "AND (FatGrams IS NULL OR FatGrams >= 0)");
            });
        });

        modelBuilder.Entity<CaloriesLogEntry>(entity =>
        {
            entity.Property(entry => entry.Name)
                .IsRequired()
                .HasMaxLength(160);
            entity.Property(entry => entry.LocalDate)
                .HasColumnType("TEXT");
            entity.Property(entry => entry.LoggedAt)
                .HasColumnType("TEXT");
            entity.Property(entry => entry.Notes)
                .HasMaxLength(500);
            entity.HasIndex(entry => entry.LocalDate);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_CaloriesEntries_Calories", "Calories > 0");
                table.HasCheckConstraint(
                    "CK_CaloriesEntries_Macros",
                    "(CarbohydratesGrams IS NULL OR CarbohydratesGrams >= 0) " +
                    "AND (ProteinGrams IS NULL OR ProteinGrams >= 0) " +
                    "AND (FatGrams IS NULL OR FatGrams >= 0)");
                table.HasCheckConstraint("CK_CaloriesEntries_Servings", "Servings IS NULL OR Servings > 0");
            });
        });

        modelBuilder.Entity<CaloriesTrackerSettings>(entity =>
        {
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_CaloriesSettings_DailyGoal",
                "DailyCalorieGoal IS NULL OR DailyCalorieGoal > 0"));
        });
    }
}

public sealed class CaloriesTrackerSettings
{
    public int Id { get; set; }

    public int? DailyCalorieGoal { get; set; }
}
