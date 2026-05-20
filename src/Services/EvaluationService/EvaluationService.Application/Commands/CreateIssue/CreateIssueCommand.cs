using EvaluationService.Application.Dtos;
using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using MediatR;

namespace EvaluationService.Application.Commands.CreateIssue;

public record CreateIssueCommand(
    string PracticeSessionId,
    string LearnerId,
    string Label,
    string Description,
    string ItemType
) : IRequest<CreateIssueResultDto>;

public sealed class CreateIssueHandler : IRequestHandler<CreateIssueCommand, CreateIssueResultDto>
{
    private readonly IEvaluationRepository _repo;

    public CreateIssueHandler(IEvaluationRepository repo) => _repo = repo;

    public async Task<CreateIssueResultDto> Handle(CreateIssueCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.PracticeSessionId))
            throw new ArgumentException("PracticeSessionId is required.");
        if (string.IsNullOrWhiteSpace(cmd.LearnerId))
            throw new ArgumentException("LearnerId is required.");
        if (string.IsNullOrWhiteSpace(cmd.Label))
            throw new ArgumentException("Label is required.");
        if (string.IsNullOrWhiteSpace(cmd.Description))
            throw new ArgumentException("Description is required.");
        if (string.IsNullOrWhiteSpace(cmd.ItemType))
            throw new ArgumentException("ItemType is required.");

        var itemType = cmd.ItemType.Trim();
        if (
            !string.Equals(itemType, "Practice", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(itemType, "Assessment", StringComparison.OrdinalIgnoreCase)
        )
            throw new ArgumentException("ItemType must be Practice or Assessment.");

        var issue = new Issue
        {
            PracticeSessionId = cmd.PracticeSessionId,
            LearnerId = cmd.LearnerId,
            Label = cmd.Label,
            Description = cmd.Description,
            ItemType = itemType,
            EditDeadline = null,
            Status = "Open",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _repo.AddIssueAsync(issue);
        await _repo.SaveChangesAsync();

        return new CreateIssueResultDto
        {
            IssueId = issue.Id,
            CreatedAt = issue.CreatedAt,
            Status = issue.Status ?? "Open",
        };
    }
}
