using AssessmentService.Application.Queries.GetPagedAssessments;

namespace AssessmentService.Application.Queries.GetAssessmentById;

public class AssessmentDetailDto : AssessmentSummaryDto
{
    public string? Descriptions { get; set; }
    public string? Goal { get; set; }
    public string? Specialty { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public List<AssessmentQuestionDto> Questions { get; set; } = new();
}

public class AssessmentQuestionDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public object? Options { get; set; } 
    public string? Explanation { get; set; }
    public decimal Points { get; set; }
}