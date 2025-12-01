using System.Text.Json.Serialization;

namespace xApiApp.ApiService.Models;

/// <summary>
/// Represents the Object of an xAPI Statement - can be Activity, Agent, StatementRef, or SubStatement
/// </summary>
[JsonConverter(typeof(Converters.StatementObjectConverter))]
public abstract class StatementObject
{
    [JsonPropertyName("objectType")]
    public abstract string ObjectType { get; }
}

/// <summary>
/// Represents an Agent when used as the Object of a Statement
/// </summary>
public class AgentAsObject : StatementObject
{
    public override string ObjectType => "Agent";

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("mbox")]
    public string? Mbox { get; set; }

    [JsonPropertyName("mbox_sha1sum")]
    public string? MboxSha1Sum { get; set; }

    [JsonPropertyName("openid")]
    public string? OpenId { get; set; }

    [JsonPropertyName("account")]
    public Account? Account { get; set; }
}

/// <summary>
/// Represents a Group when used as the Object of a Statement
/// </summary>
public class GroupAsObject : StatementObject
{
    public override string ObjectType => "Group";

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("member")]
    public List<Agent>? Member { get; set; }

    // Inverse Functional Identifiers for Identified Groups
    [JsonPropertyName("mbox")]
    public string? Mbox { get; set; }

    [JsonPropertyName("mbox_sha1sum")]
    public string? MboxSha1Sum { get; set; }

    [JsonPropertyName("openid")]
    public string? OpenId { get; set; }

    [JsonPropertyName("account")]
    public Account? Account { get; set; }
}

/// <summary>
/// Represents an Activity as the Object of a Statement
/// </summary>
public class Activity : StatementObject
{
    public override string ObjectType => "Activity";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("definition")]
    public ActivityDefinition? Definition { get; set; }
}

/// <summary>
/// Represents the definition of an Activity
/// </summary>
public class ActivityDefinition
{
    [JsonPropertyName("name")]
    public Dictionary<string, string>? Name { get; set; }

    [JsonPropertyName("description")]
    public Dictionary<string, string>? Description { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("moreInfo")]
    public string? MoreInfo { get; set; }

    [JsonPropertyName("interactionType")]
    public string? InteractionType { get; set; }

    [JsonPropertyName("correctResponsesPattern")]
    public List<string>? CorrectResponsesPattern { get; set; }

    [JsonPropertyName("choices")]
    public List<InteractionComponent>? Choices { get; set; }

    [JsonPropertyName("scale")]
    public List<InteractionComponent>? Scale { get; set; }

    [JsonPropertyName("source")]
    public List<InteractionComponent>? Source { get; set; }

    [JsonPropertyName("target")]
    public List<InteractionComponent>? Target { get; set; }

    [JsonPropertyName("steps")]
    public List<InteractionComponent>? Steps { get; set; }

    [JsonPropertyName("extensions")]
    public Dictionary<string, object>? Extensions { get; set; }
}

/// <summary>
/// Represents an interaction component (for interaction activities)
/// </summary>
public class InteractionComponent
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("description")]
    public Dictionary<string, string>? Description { get; set; }
}

/// <summary>
/// Represents a reference to another Statement
/// </summary>
public class StatementRef : StatementObject
{
    public override string ObjectType => "StatementRef";

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

/// <summary>
/// Represents a SubStatement (nested Statement)
/// </summary>
public class SubStatement : StatementObject
{
    public override string ObjectType => "SubStatement";

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

    [JsonPropertyName("attachments")]
    public List<Attachment>? Attachments { get; set; }
}

