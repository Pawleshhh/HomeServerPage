using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Fridge;

public class FridgeService(IDbContextFactory<FridgeDbContext> dbContextFactory) : IFridgeService
{
    public async Task<FridgeItem?> GetItemAsync(int id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.FridgeItems.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<FridgeItem>> GetItemsAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.FridgeItems.ToListAsync();
    }

    public async Task<List<FridgeItemTemplate>> GetTemplatesAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.FridgeItemTemplates
            .OrderBy(template => template.Name)
            .ToListAsync();
    }

    public async Task<FridgeItem> AddItemAsync(FridgeItem item, bool saveAsTemplate = false)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        if (saveAsTemplate)
        {
            var template = FridgeItemTemplate.FromFridgeItem(item);
            var existingTemplate = await dbContext.FridgeItemTemplates
                .FirstOrDefaultAsync(existing => existing.Name == template.Name);

            if (existingTemplate is null)
            {
                await dbContext.FridgeItemTemplates.AddAsync(template);
            }
            else
            {
                dbContext.Entry(existingTemplate).CurrentValues.SetValues(template with { Id = existingTemplate.Id });
            }
        }

        var entry = await dbContext.AddAsync(item);
        await dbContext.SaveChangesAsync();
        return entry.Entity;
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
