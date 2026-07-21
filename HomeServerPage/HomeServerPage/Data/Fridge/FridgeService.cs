using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Fridge;

public class FridgeService(IDbContextFactory<FridgeDbContext> dbContextFactory) : IFridgeService
{
    public async Task<List<FridgeItem>> GetItemsAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.FridgeItems.ToListAsync();
    }

    public async Task AddItemAsync(FridgeItem item)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.AddAsync(item);
        await dbContext.SaveChangesAsync();
    }
}
