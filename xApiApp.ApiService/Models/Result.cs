using System.Text.Json.Serialization;

namespace xApiApp.ApiService.Models;

/// <summary>
/// Represents the Result of an xAPI Statement - details about the outcome
/// </summary>
public class Result
{
    [JsonPropertyName("score")]
    public Score? Score { get; set; }

    [JsonPropertyName("success")]
    public bool? Success { get; set; }

    [JsonPropertyName("completion")]
    public bool? Completion { get; set; }

    [JsonPropertyName("response")]
    public string? Response { get; set; }

    [JsonPropertyName("duration")]
    public string? Duration { get; set; }

    [JsonPropertyName("extensions")]
    public Dictionary<string, object>? Extensions { get; set; }
}

/// <summary>
/// Represents a score in a Result
/// </summary>
public class Score
{
    [JsonPropertyName("scaled")]
    public double? Scaled { get; set; }

    [JsonPropertyName("raw")]
    public double? Raw { get; set; }

    [JsonPropertyName("min")]
    public double? Min { get; set; }

    [JsonPropertyName("max")]
    public double? Max { get; set; }
}

