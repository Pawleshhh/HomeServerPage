namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public record DeepSkyObject(string Symbol, string Catalog, double Width, double Height)
{
    public int Id { get; set; }
}
