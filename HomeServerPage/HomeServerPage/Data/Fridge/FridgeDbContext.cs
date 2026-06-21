using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Fridge;

public class FridgeDbContext : DbContext
{

    public FridgeDbContext(DbContextOptions<FridgeDbContext> options) : base(options)
    {
    }

    public DbSet<FridgeItem> FridgeItems { get; set; }

}
