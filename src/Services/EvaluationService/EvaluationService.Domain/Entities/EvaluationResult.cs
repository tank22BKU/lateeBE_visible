namespace EvaluationService.Domain.Entities;

public class EvaluationResult
{
    public string ResultId { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = null!;
    public string ClinicalCaseId { get; set; } = null!;
    public string ModuleId { get; set; } = "EPA_STANDARD_V1";
    public string? VpConversationLog { get; set; } 
    public string? AiReasoningLog { get; set; }   
    public string? FinalDiagnosis { get; set; }
    public decimal OverallScore { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relationships
    public virtual ICollection<EpaScore> EpaScores { get; set; } = new List<EpaScore>();
    public virtual ICollection<EvaluationWarning> Warnings { get; set; } = new List<EvaluationWarning>();
}