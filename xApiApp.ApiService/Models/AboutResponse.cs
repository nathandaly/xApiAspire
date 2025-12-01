using System.Text.Json.Serialization;

namespace xApiApp.ApiService.Models;

/// <summary>
/// Represents the response from the /about endpoint
/// </summary>
public class AboutResponse
{
    [JsonPropertyName("version")]
    public List<string> Version { get; set; } = new();

    [JsonPropertyName("extensions")]
    public Dictionary<string, object>? Extensions { get; set; }
}

