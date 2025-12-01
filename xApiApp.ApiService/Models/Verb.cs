using System.Text.Json.Serialization;

namespace xApiApp.ApiService.Models;

/// <summary>
/// Represents a Verb in an xAPI Statement - defines the action performed
/// </summary>
public class Verb
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("display")]
    public Dictionary<string, string>? Display { get; set; }
}

