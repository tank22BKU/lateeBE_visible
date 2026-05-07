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
    public string Id { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public object? QuestionOption { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public decimal Points { get; set; }
}