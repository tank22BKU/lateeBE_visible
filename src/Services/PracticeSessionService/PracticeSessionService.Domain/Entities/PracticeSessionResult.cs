using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PracticeSessionService.Domain.Entities;

[Table("evaluation_results")]
public class PracticeSessionResult
{
    [Key]
    [Column("result_id")]
    public string ResultId { get; set; } = default!;

    [Column("session_id")]
    public string SessionId { get; set; } = default!;

    [Column("user_id")]
    public string UserId { get; set; } = default!;

    [Column("clinical_case_id")]
    public string ClinicalCaseId { get; set; } = default!;

    [Column("module_id")]
    public string ModuleId { get; set; } = default!;

    [Column("vp_conversation_log")]
    public string? VpConversationLog { get; set; }

    [Column("ai_reasoning_log")]
    public string? AiReasoningLog { get; set; }

    [Column("final_diagnosis")]
    public string? FinalDiagnosis { get; set; }

    [Column("overall_score")]
    public decimal OverallScore { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<EvaluationWarning> Warnings { get; set; } = [];
}