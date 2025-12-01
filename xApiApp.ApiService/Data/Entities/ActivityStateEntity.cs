using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace xApiApp.ApiService.Data.Entities;

[Table("activity_states")]
[Index(nameof(ActivityId))]
[Index(nameof(RegistrationId))]
[Index(nameof(StateId))]
[Index(nameof(Updated))]
public class ActivityStateEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(2048)]
    [Column("state_id")]
    public string StateId { get; set; } = string.Empty;

    [Required]
    [Column("updated")]
    public DateTimeOffset Updated { get; set; }

    [Required]
    [MaxLength(2048)]
    [Column("activity_id")]
    public string ActivityId { get; set; } = string.Empty;

    [Required]
    [MaxLength(36)]
    [Column("registration_id")]
    public string RegistrationId { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("content_type")]
    public string? ContentType { get; set; }

    [MaxLength(50)]
    [Column("etag")]
    public string? Etag { get; set; }

    [Required]
    [Column("agent_id")]
    public int AgentId { get; set; }

    [ForeignKey(nameof(AgentId))]
    public virtual AgentEntity Agent { get; set; } = null!;

    [Column("json_state", TypeName = "TEXT")]
    public string? JsonState { get; set; }

    [MaxLength(255)]
    [Column("state_path")]
    public string? StatePath { get; set; }
}

