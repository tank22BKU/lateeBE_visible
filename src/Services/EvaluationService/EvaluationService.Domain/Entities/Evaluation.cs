namespace EvaluationService.Domain.Entities;

public class Evaluation
{
    public string   Id                { get; set; } = default!;
    public string   EpaId             { get; set; } = default!;
    public string   PracticeSessionId { get; set; } = default!;
    public decimal? Score             { get; set; }
    public int?     Duration          { get; set; }
    public DateTime CreatedAt         { get; set; }
	public string?  FeedbackDetail    { get; set; }
    public int?     EntrustmentLevel  { get; set; }
    public string?  RubricVersion     { get; set; }

    public ICollection<EvaluationEpaScore> EpaScores { get; set; } = [];
}