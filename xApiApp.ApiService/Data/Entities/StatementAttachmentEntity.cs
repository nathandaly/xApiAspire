using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace xApiApp.ApiService.Data.Entities;

[Table("statement_attachments")]
public class StatementAttachmentEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("statement_id")]
    public int StatementId { get; set; }

    [ForeignKey(nameof(StatementId))]
    public virtual StatementEntity Statement { get; set; } = null!;

    [Column("canonical_data", TypeName = "TEXT")]
    public string CanonicalData { get; set; } = "{}";

    [MaxLength(255)]
    [Column("payload_path")]
    public string? PayloadPath { get; set; }
}

