using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PracticeSessionService.Domain.Entities;

[Table("practice_sessions")]
public class PracticeSession
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = default!;

    [Column("learnerid")]
    public string LearnerId { get; set; } = default!;

    [Column("clinicalcaseid")]
    public string ClinicalCaseId { get; set; } = default!;

    [Column("start_time")]
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    [Column("end_time")]
    public DateTime? EndTime { get; set; }

    [Column("duration")]
    public int? Duration { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("status")]
    public string Status { get; set; } = "Practicing";

    public List<PracticeSessionResult> EvaluationResults { get; set; } = [];
}
