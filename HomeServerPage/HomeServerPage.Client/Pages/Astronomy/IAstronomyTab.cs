namespace HomeServerPage.Client.Pages.Astronomy;

public interface IAstronomyTab
{
    string Key { get; }

    string Label { get; }

    string PanelId { get; }

    Type ComponentType { get; }
}

public sealed record AstronomyTab(
    string Key,
    string Label,
    string PanelId,
    Type ComponentType) : IAstronomyTab;
