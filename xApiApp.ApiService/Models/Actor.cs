using System.Text.Json.Serialization;

namespace xApiApp.ApiService.Models;

/// <summary>
/// Represents an Actor (Agent or Group) in an xAPI Statement
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "objectType")]
[JsonDerivedType(typeof(Agent), "Agent")]
[JsonDerivedType(typeof(Group), "Group")]
public abstract class Actor
{
    [JsonPropertyName("objectType")]
    public abstract string ObjectType { get; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>
/// Represents an Agent (individual) in an xAPI Statement
/// </summary>
public class Agent : Actor
{
    public override string ObjectType => "Agent";

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
/// Represents a Group (collection of Agents) in an xAPI Statement
/// </summary>
public class Group : Actor
{
    public override string ObjectType => "Group";

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
/// Represents an Account object for Agent/Group identification
/// </summary>
public class Account
{
    [JsonPropertyName("homePage")]
    public string? HomePage { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

