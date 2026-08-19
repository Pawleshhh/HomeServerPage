namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public record TelescopeEyepiece(
    double FocalLength,
    double FieldOfView,
    double BarrelDiameter)
{
    public int Id { get; init; }

    public TelescopeEyepiece(
        int Id,
        double FocalLength,
        double FieldOfView,
        double BarrelDiameter)
        : this(FocalLength, FieldOfView, BarrelDiameter)
    {
        this.Id = Id;
    }
}