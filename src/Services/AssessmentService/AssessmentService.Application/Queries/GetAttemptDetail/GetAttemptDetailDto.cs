namespace AssessmentService.Application.Queries.GetAttemptDetails;

// DTOs
public class GetAttemptDetailDto
{
    public string AttemptId { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public bool IsPassed { get; set; }
    public int CorrectCount { get; set; }
    public List<QuestionResultDto> Questions { get; set; } = new();
}

public class QuestionResultDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? UserAnswerId { get; set; }
    public string? CorrectAnswerId { get; set; }
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
    public List<OptionResultDto> Options { get; set; } = new();
}

public class OptionResultDto
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}