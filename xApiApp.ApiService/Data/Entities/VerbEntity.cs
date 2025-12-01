using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace xApiApp.ApiService.Data.Entities;

[Table("verbs")]
[Index(nameof(VerbId), IsUnique = true)]
public class VerbEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(2048)]
    [Column("verb_id")]
    public string VerbId { get; set; } = string.Empty;

    [Column("canonical_data", TypeName = "TEXT")]
    public string CanonicalData { get; set; } = "{}";
}

