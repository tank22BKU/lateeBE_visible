namespace EvaluationService.Domain.Entities;

public class EpaScore
{
    public string ScoreId { get; set; } = Guid.NewGuid().ToString("N");
    public string ResultId { get; set; } = null!;
    public string EpaId { get; set; } = null!; 
    public int EntrustmentLevel { get; set; } 
    public decimal NumericalScore { get; set; } 
    public string FeedbackDetail { get; set; } = null!; 

    public EvaluationResult EvaluationResult { get; set; } = null!;
}