using System;

namespace EvaluationService.Domain.Entities;

public class PracticeSession
{
    public string Id { get; set; } = default!;
    public string LearnerId { get; set; } = default!;
    public string ClinicalCaseId { get; set; } = default!;
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public int? Duration { get; set; }
    public bool IsActive { get; set; } = true;
    public string Status { get; set; } = "Practicing";
}