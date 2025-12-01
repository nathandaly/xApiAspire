using System.Text.Json.Serialization;

namespace xApiApp.ApiService.Models;

/// <summary>
/// Represents the result of a GET statements query
/// </summary>
public class StatementResult
{
    [JsonPropertyName("statements")]
    public List<Statement> Statements { get; set; } = new();

    [JsonPropertyName("more")]
    public string? More { get; set; }
}

