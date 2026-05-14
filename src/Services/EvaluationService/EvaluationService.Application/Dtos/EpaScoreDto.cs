namespace EvaluationService.Application.Dtos;

public class EpaScoreDto
{
    public string       EpaId            { get; set; } = string.Empty;
    public int          NumericalScore   { get; set; }
    public int          EntrustmentLevel { get; set; }
    public string       FeedbackDetail   { get; set; } = string.Empty;
    public List<string> EvidenceCited    { get; set; } = [];
    public List<string> FailurePatterns  { get; set; } = [];
    public List<string> SafetyFlags      { get; set; } = [];
}