namespace HomeServerPage.Data.PublicTransport;

public static class PublicTransportLineColors
{
    private static readonly string[] ColorPalette =
    [
        "#e6194b", "#3cb44b", "#4363d8", "#f58231", "#911eb4",
        "#46f0f0", "#f032e6", "#bcf60c", "#fabebe", "#008080",
        "#e6beff", "#9a6324", "#800000", "#808000", "#000075"
    ];

    public static string GetColor(string lineNumber)
    {
        var hash = StringComparer.Ordinal.GetHashCode(lineNumber);
        var paletteIndex = (int)((uint)hash % ColorPalette.Length);
        return ColorPalette[paletteIndex];
    }
}
