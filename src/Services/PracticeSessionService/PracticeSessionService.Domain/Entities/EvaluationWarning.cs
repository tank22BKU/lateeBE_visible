using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PracticeSessionService.Domain.Entities;

[Table("evaluation_warnings")]
public class EvaluationWarning
{
    [Key]
    [Column("warning_id")]
    public string WarningId { get; set; } = default!;

    [Column("result_id")]
    public string ResultId { get; set; } = default!;

    [Column("label")]
    public string? Label { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ResultId))]
    public PracticeSessionResult PracticeSessionResult { get; set; } = default!;
}