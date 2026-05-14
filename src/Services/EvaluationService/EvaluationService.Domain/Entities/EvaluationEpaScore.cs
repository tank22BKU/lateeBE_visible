namespace EvaluationService.Domain.Entities;

public class EvaluationEpaScore
{
    public string  Id               { get; set; } = Guid.NewGuid().ToString("N");
    public string  EvaluationId     { get; set; } = default!;
    public string  EpaId            { get; set; } = default!;
    public int     NumericalScore   { get; set; }
    public int     EntrustmentLevel { get; set; }
    public string? FeedbackDetail   { get; set; }
    public List<string> EvidenceCited   { get; set; } = [];
    public List<string> FailurePatterns { get; set; } = [];
    public List<string> SafetyFlags     { get; set; } = [];
    public DateTime CreatedAt       { get; set; } = DateTime.UtcNow; 

    public Evaluation? Evaluation   { get; set; }
}