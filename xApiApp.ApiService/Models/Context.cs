using System.Text.Json.Serialization;

namespace xApiApp.ApiService.Models;

/// <summary>
/// Represents the Context of an xAPI Statement - provides additional meaning
/// </summary>
public class Context
{
    [JsonPropertyName("registration")]
    public string? Registration { get; set; }

    [JsonPropertyName("instructor")]
    public Actor? Instructor { get; set; }

    [JsonPropertyName("team")]
    public Group? Team { get; set; }

    [JsonPropertyName("contextActivities")]
    public ContextActivities? ContextActivities { get; set; }

    [JsonPropertyName("revision")]
    public string? Revision { get; set; }

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("statement")]
    public StatementRef? Statement { get; set; }

    [JsonPropertyName("extensions")]
    public Dictionary<string, object>? Extensions { get; set; }
}

/// <summary>
/// Represents context activities (parent, grouping, category, other)
/// </summary>
public class ContextActivities
{
    [JsonPropertyName("parent")]
    public List<Activity>? Parent { get; set; }

    [JsonPropertyName("grouping")]
    public List<Activity>? Grouping { get; set; }

    [JsonPropertyName("category")]
    public List<Activity>? Category { get; set; }

    [JsonPropertyName("other")]
    public List<Activity>? Other { get; set; }
}

