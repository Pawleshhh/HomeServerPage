namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public record TelescopeItem(
    string Name,
    TelescopeType Type,
    double Aperture,
    double FocalLength,
    double ApertureSpeed)
{
    public int Id { get; init; }

    public TelescopeItem(
        int Id, 
        string Name,
        TelescopeType Type,
        double Aperture,
        double FocalLength,
        double ApertureSpeed)
        : this(Name, Type, Aperture, FocalLength, ApertureSpeed)
    {
        this.Id = Id;
    }
}
