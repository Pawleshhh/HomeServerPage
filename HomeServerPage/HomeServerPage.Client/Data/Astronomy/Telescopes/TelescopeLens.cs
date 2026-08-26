namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public record TelescopeLens(
    string Name,
    double Multiplier,
    BarellSize BarellSize)
{
    public int Id { get; set; }

    public LensType Type => Multiplier >= 1 ? LensType.Barlow : LensType.Shapley;
}

public enum LensType
{
    Barlow,
    Shapley,
    Undefined
}