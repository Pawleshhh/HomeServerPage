using System.Text.Json;

namespace HomeServerPage.Helpers;

public static class ResourcesHelper
{

    public static JsonDocument? GetJsonDocument(string resourceName)
    {
        using var stream = typeof(ResourcesHelper).Assembly.GetManifestResourceStream(
            $"HomeServerPage.wwwroot.data.{resourceName}.json");

        if (stream is null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        using var document = JsonDocument.Parse(reader.ReadToEnd());

        return document;
    }

}
