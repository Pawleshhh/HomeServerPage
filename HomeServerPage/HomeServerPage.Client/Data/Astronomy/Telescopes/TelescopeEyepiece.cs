namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public record TelescopeEyepiece(
    string Name,
    double FocalLength,
    double FieldOfView,
    BarellSizes BarellSize)
{
    public int Id { get; set; }
}