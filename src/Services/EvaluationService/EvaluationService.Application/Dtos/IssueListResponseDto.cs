namespace EvaluationService.Application.Dtos;

public class IssueListResponseDto
{
    public List<IssueItemDto> Items { get; set; } = [];
}

public class IssueItemDto
{
    public string IssueId { get; set; } = string.Empty;

    public string LearnerId { get; set; } = string.Empty;

    public string LearnerName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? Label { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = "Open";

    public IssueExpertFeedbackDto? ExpertFeedback { get; set; }
}

public class IssueExpertFeedbackDto
{
    public string ExpertId { get; set; } = string.Empty;

    public string ExpertName { get; set; } = string.Empty;

    public string Feedback { get; set; } = string.Empty;
}

public class CreateIssueResultDto
{
    public string IssueId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = "Open";
}