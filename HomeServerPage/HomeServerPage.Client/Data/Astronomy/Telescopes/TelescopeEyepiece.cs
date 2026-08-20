namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public record TelescopeEyepiece(
    string Name,
    double FocalLength,
    double FieldOfView,
    BarellSize BarellSize)
{
    public int Id { get; set; }
}