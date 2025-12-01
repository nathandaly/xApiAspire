using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace xApiApp.ApiService.Data.Entities;

[Table("statements")]
[Index(nameof(StatementId), IsUnique = true)]
[Index(nameof(Stored))]
[Index(nameof(Timestamp))]
[Index(nameof(ActorId))]
[Index(nameof(VerbId))]
[Index(nameof(ObjectActivityId))]
[Index(nameof(ObjectAgentId))]
[Index(nameof(AuthorityId))]
public class StatementEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(36)]
    [Column("statement_id")]
    public string StatementId { get; set; } = string.Empty;

    // Actor (required)
    [Required]
    [Column("actor_id")]
    public int ActorId { get; set; }

    [ForeignKey(nameof(ActorId))]
    public virtual AgentEntity Actor { get; set; } = null!;

    // Verb (required)
    [Required]
    [Column("verb_id")]
    public int VerbId { get; set; }

    [ForeignKey(nameof(VerbId))]
    public virtual VerbEntity Verb { get; set; } = null!;

    // Object - can be Activity, Agent, SubStatement, or StatementRef
    [Column("object_activity_id")]
    public int? ObjectActivityId { get; set; }

    [ForeignKey(nameof(ObjectActivityId))]
    public virtual ActivityEntity? ObjectActivity { get; set; }

    [Column("object_agent_id")]
    public int? ObjectAgentId { get; set; }

    [ForeignKey(nameof(ObjectAgentId))]
    public virtual AgentEntity? ObjectAgent { get; set; }

    [Column("object_substatement_id")]
    public int? ObjectSubstatementId { get; set; }

    [ForeignKey(nameof(ObjectSubstatementId))]
    public virtual StatementEntity? ObjectSubstatement { get; set; }

    [MaxLength(36)]
    [Column("object_statementref")]
    public string? ObjectStatementRef { get; set; }

    // Result (optional, stored as JSON)
    [Column("result", TypeName = "TEXT")]
    public string? Result { get; set; }

    // Context (optional)
    [Column("context_registration")]
    [MaxLength(36)]
    public string? ContextRegistration { get; set; }

    [Column("context_instructor_id")]
    public int? ContextInstructorId { get; set; }

    [ForeignKey(nameof(ContextInstructorId))]
    public virtual AgentEntity? ContextInstructor { get; set; }

    [Column("context_team_id")]
    public int? ContextTeamId { get; set; }

    [ForeignKey(nameof(ContextTeamId))]
    public virtual AgentEntity? ContextTeam { get; set; }

    [Column("context_revision")]
    [MaxLength(255)]
    public string? ContextRevision { get; set; }

    [Column("context_platform")]
    [MaxLength(255)]
    public string? ContextPlatform { get; set; }

    [Column("context_language")]
    [MaxLength(10)]
    public string? ContextLanguage { get; set; }

    [Column("context_statement")]
    [MaxLength(36)]
    public string? ContextStatement { get; set; }

    [Column("context_extensions", TypeName = "TEXT")]
    public string? ContextExtensions { get; set; }

    // Context Activities (many-to-many)
    [InverseProperty(nameof(ContextActivityEntity.Statement))]
    public virtual ICollection<ContextActivityEntity> ContextActivities { get; set; } = new List<ContextActivityEntity>();

    // Timestamps
    [Required]
    [Column("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [Required]
    [Column("stored")]
    public DateTimeOffset Stored { get; set; }

    // Authority (optional)
    [Column("authority_id")]
    public int? AuthorityId { get; set; }

    [ForeignKey(nameof(AuthorityId))]
    public virtual AgentEntity? Authority { get; set; }

    // Version
    [MaxLength(20)]
    [Column("version")]
    public string Version { get; set; } = "2.0.0";

    // Attachments
    [InverseProperty(nameof(StatementAttachmentEntity.Statement))]
    public virtual ICollection<StatementAttachmentEntity> Attachments { get; set; } = new List<StatementAttachmentEntity>();
}

[Table("context_activities")]
[Index(nameof(StatementId))]
[Index(nameof(ActivityId))]
public class ContextActivityEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("statement_id")]
    public int StatementId { get; set; }

    [ForeignKey(nameof(StatementId))]
    public virtual StatementEntity Statement { get; set; } = null!;

    [Required]
    [Column("activity_id")]
    public int ActivityId { get; set; }

    [ForeignKey(nameof(ActivityId))]
    public virtual ActivityEntity Activity { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    [Column("type")] // parent, grouping, category, other
    public string Type { get; set; } = string.Empty;
}

