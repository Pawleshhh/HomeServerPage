using System.Net;
using System.Text.Json;

namespace HomeServerPage.Client.Data.Locations;

internal static class LocationHttpResponse
{
    public static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var detail = GetProblemDetail(content);
        throw new HttpRequestException(
            $"Location request failed: {detail}",
            null,
            response.StatusCode);
    }

    private static string GetProblemDetail(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "The server returned an empty error response.";
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (document.RootElement.TryGetProperty("detail", out var detail)
                    && detail.ValueKind == JsonValueKind.String)
                {
                    return detail.GetString()!;
                }

                if (document.RootElement.TryGetProperty("title", out var title)
                    && title.ValueKind == JsonValueKind.String)
                {
                    return title.GetString()!;
                }
            }
        }
        catch (JsonException)
        {
        }

        return content;
    }
}
