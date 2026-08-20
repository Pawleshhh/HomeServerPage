namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public record TelescopeItem(
    string Name,
    TelescopeType Type,
    double Aperture,
    double FocalLength,
    double ApertureSpeed)
{
    public int Id { get; set; }
}
