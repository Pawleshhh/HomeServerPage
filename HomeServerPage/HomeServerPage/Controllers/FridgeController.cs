using HomeServerPage.Data.Fridge;
using Microsoft.AspNetCore.Mvc;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FridgeController(IFridgeService fridgeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<FridgeItem>>> GetItems()
    {
        var items = await fridgeService.GetItemsAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> AddItem([FromBody] FridgeItem item)
    {
        await fridgeService.AddItemAsync(item);
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] FridgeItem item)
    {
        var updated = await fridgeService.UpdateItemAsync(item with { Id = id });
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveItem(int id)
    {
        var removed = await fridgeService.RemoveItemAsync(id);
        if (!removed)
        {
            return NotFound();
        }

        return NoContent();
    }
}
