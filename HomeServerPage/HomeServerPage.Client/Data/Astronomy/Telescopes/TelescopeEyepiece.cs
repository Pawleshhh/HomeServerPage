namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public record TelescopeEyepiece(
    double FocalLength,
    double FieldOfView,
    double BarrelDiameter)
{
    public int Id { get; set; }
}