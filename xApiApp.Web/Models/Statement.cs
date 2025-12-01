using System.Text.Json.Serialization;

namespace xApiApp.Web.Models;

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
}

public class Actor
{
    [JsonPropertyName("objectType")]
    public string? ObjectType { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("mbox")]
    public string? Mbox { get; set; }
}

public class Verb
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("display")]
    public Dictionary<string, string>? Display { get; set; }
}

public class StatementObject
{
    [JsonPropertyName("objectType")]
    public string? ObjectType { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class Result
{
    [JsonPropertyName("success")]
    public bool? Success { get; set; }

    [JsonPropertyName("completion")]
    public bool? Completion { get; set; }

    [JsonPropertyName("score")]
    public Score? Score { get; set; }
}

public class Score
{
    [JsonPropertyName("scaled")]
    public double? Scaled { get; set; }

    [JsonPropertyName("raw")]
    public double? Raw { get; set; }
}

public class Context
{
    [JsonPropertyName("registration")]
    public string? Registration { get; set; }
}

public class StatementResult
{
    [JsonPropertyName("statements")]
    public List<Statement> Statements { get; set; } = new();

    [JsonPropertyName("more")]
    public string? More { get; set; }
}

public class AboutResponse
{
    [JsonPropertyName("version")]
    public List<string> Version { get; set; } = new();

    [JsonPropertyName("extensions")]
    public Dictionary<string, object>? Extensions { get; set; }
}

