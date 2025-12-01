using System.Text.Json.Serialization;

namespace xApiApp.ApiService.Models;

/// <summary>
/// Represents an Attachment to an xAPI Statement
/// </summary>
public class Attachment
{
    [JsonPropertyName("usageType")]
    public string? UsageType { get; set; }

    [JsonPropertyName("display")]
    public Dictionary<string, string>? Display { get; set; }

    [JsonPropertyName("description")]
    public Dictionary<string, string>? Description { get; set; }

    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }

    [JsonPropertyName("length")]
    public long? Length { get; set; }

    [JsonPropertyName("sha2")]
    public string? Sha2 { get; set; }

    [JsonPropertyName("fileUrl")]
    public string? FileUrl { get; set; }
}

