using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Fridge;

public class FridgeDbContext : DbContext
{

    public FridgeDbContext(DbContextOptions<FridgeDbContext> options) : base(options)
    {
    }

    public DbSet<FridgeItem> FridgeItems { get; set; }

    public DbSet<FridgeItemTemplate> FridgeItemTemplates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FridgeItemTemplate>(entity =>
        {
            entity.Property(template => template.Name)
                .IsRequired()
                .UseCollation("NOCASE");
            entity.HasIndex(template => template.Name)
                .IsUnique();
        });
    }

}
