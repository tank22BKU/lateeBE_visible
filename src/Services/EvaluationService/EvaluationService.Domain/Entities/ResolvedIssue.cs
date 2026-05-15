namespace EvaluationService.Domain.Entities;

public class ResolvedIssue
{
    public string IssueId  { get; set; } = default!;
    public string ExpertId { get; set; } = default!;
    public string? Feedback { get; set; }
}
