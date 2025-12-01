using System.Text.Json.Serialization;

namespace xApiApp.ApiService.Models;

/// <summary>
/// Represents an xAPI Statement - the core data structure for tracking learning experiences
/// </summary>
public class Statement
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("actor")]
    public Actor? Actor { get; set; }

    [JsonPropertyName("verb")]
    public Verb? Verb { get; set; }

    [JsonPropertyName("object")]
    public StatementObject? Object { get; set; }

    [JsonPropertyName("result")]
    public Result? Result { get; set; }

    [JsonPropertyName("context")]
    public Context? Context { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset? Timestamp { get; set; }

    [JsonPropertyName("stored")]
    public DateTimeOffset? Stored { get; set; }

    [JsonPropertyName("authority")]
    public Actor? Authority { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("attachments")]
    public List<Attachment>? Attachments { get; set; }
}

