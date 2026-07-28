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

    public async Task<bool> UpdateItemAsync(FridgeItem item)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var entity = await dbContext.FridgeItems.FindAsync(item.Id);
        if (entity is null)
        {
            return false;
        }

        dbContext.Entry(entity).CurrentValues.SetValues(item);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveItemAsync(int id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var entity = await dbContext.FridgeItems.FindAsync(id);
        if (entity is null)
        {
            return false;
        }

        dbContext.FridgeItems.Remove(entity);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
