using System.ComponentModel.DataAnnotations;
using HomeServerPage.Data.Fridge;
using HomeServerPage.Shared.Extensions;

namespace HomeServerPage.Client.Pages;

public class FridgeItemModel : IValidatableObject
{
    [Required]
    public string? Name { get; set; }

    [Range(0, double.MaxValue)]
    public double QuanitityValue { get; set; }

    public QuantityType QuantityType { get; set; }

    public DateTime AddedDate { get; set; }

    public DateTime? OpenDate { get; set; }

    public DateTime ExpirationDate { get; set; }

    public int? TimeAfterOpen { get; set; }

    public static FridgeItemModel CreateClearEditedFridgeItem(DateTime now)
        => new FridgeItemModel
            {
                Name = "Name",
                QuanitityValue = 1,
                QuantityType = QuantityType.Unit,
                AddedDate = now,
                OpenDate = null,
                ExpirationDate = now.AddDays(1),
                TimeAfterOpen = 0
            };

    public static FridgeItemModel FromFridgeItem(FridgeItem item)
        => new FridgeItemModel
            {
                Name = item.Name,
                QuanitityValue = item.QuantityValue,
                QuantityType = item.QuantityType,
                AddedDate = item.AddedDate,
                OpenDate = item.OpenDate,
                ExpirationDate = item.ExpirationDate,
                TimeAfterOpen = item.TimeAfterOpen.HasValue ? (int)item.TimeAfterOpen.Value.TotalHours : null
            };

    public FridgeItem ToFridgeItem()
        => new(
            0,
            Name ?? throw new NullReferenceException(),
            QuanitityValue,
            QuantityType,
            AddedDate,
            OpenDate,
            ExpirationDate,
            TimeAfterOpen.Value()?.Pipe(TimeSpan.FromHours));

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ExpirationDate.Date < AddedDate.Date)
        {
            yield return new ValidationResult(
                $"Expiration Date cannot be earlier than Added Date",
                new[] { nameof(ExpirationDate) });
        }

        if (OpenDate is not null && OpenDate.Value.Date < AddedDate.Date)
        {
            yield return new ValidationResult(
                $"Open Date cannot be earlier than Added Date",
                new[] { nameof(OpenDate) });
        }
    }
}
