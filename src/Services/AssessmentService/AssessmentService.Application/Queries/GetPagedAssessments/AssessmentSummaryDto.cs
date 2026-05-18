namespace AssessmentService.Application.Queries.GetPagedAssessments;

public class AssessmentSummaryDto
{
    public string AssessmentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Subtopic { get; set; } = string.Empty;
    public string Descriptions { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = string.Empty;
    public int MaxAttempts { get; set; }
    public int NumQuestions { get; set; }
    public decimal MaxScore { get; set; }
    public decimal PassingScorePercentage { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}