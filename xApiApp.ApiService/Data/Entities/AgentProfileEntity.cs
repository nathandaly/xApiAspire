using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace xApiApp.ApiService.Data.Entities;

[Table("agent_profiles")]
[Index(nameof(ProfileId))]
[Index(nameof(AgentId))]
[Index(nameof(Updated))]
public class AgentProfileEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(2048)]
    [Column("profile_id")]
    public string ProfileId { get; set; } = string.Empty;

    [Required]
    [Column("updated")]
    public DateTimeOffset Updated { get; set; }

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

    [Column("json_profile", TypeName = "TEXT")]
    public string? JsonProfile { get; set; }

    [MaxLength(255)]
    [Column("profile_path")]
    public string? ProfilePath { get; set; }
}

