namespace EvaluationService.Application.Queries.GetPracticeHistory;

public class PracticeHistoryResponse
{
    public string LearnerId { get; set; } = default!;
    public string PatientId { get; set; } = default!;
    public List<PracticeHistoryItemDto> Items { get; set; } = new();
}

public class PracticeHistoryItemDto
{
    public string PracticeSessionId { get; set; } = default!;
    public string? EvaluationId { get; set; }
    public decimal? Score { get; set; }
    public int? PureEpaScore { get; set; }
    public int? EntrustmentLevel { get; set; }
    public string? FinalDiagnosis { get; set; }
    public int? Duration { get; set; }
    public string? DiagnosisMatch { get; set; }
    public string? RubricVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = default!;
    public string? FeedbackId { get; set; }
}
