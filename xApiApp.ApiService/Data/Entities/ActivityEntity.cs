using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace xApiApp.ApiService.Data.Entities;

[Table("activities")]
[Index(nameof(ActivityId))]
public class ActivityEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(2048)]
    [Column("activity_id")]
    public string ActivityId { get; set; } = string.Empty;

    [Column("canonical_data", TypeName = "TEXT")]
    public string CanonicalData { get; set; } = "{}";
}

