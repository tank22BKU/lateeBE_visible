using EvaluationService.Application.Dtos;
using EvaluationService.Application.Orchestrators;
using MediatR;

namespace EvaluationService.Application.Commands.SubmitEvaluation;

public record SubmitEvaluationCommand(
    string PracticeSessionId,
    string LearnerId,
    string? FinalDiagnosis,
    string? VpConversationLog,
    string? AiReasoningLog,
    string? DiscussionType,
    string? ModuleId,
    List<WarningDto> Warnings
) : IRequest<SubmitEvaluationResultDto>;

public sealed class SubmitEvaluationHandler
    : IRequestHandler<SubmitEvaluationCommand, SubmitEvaluationResultDto>
{
    private readonly EvaluationOrchestrator _orchestrator;

    public SubmitEvaluationHandler(EvaluationOrchestrator orchestrator) =>
        _orchestrator = orchestrator;

    public async Task<SubmitEvaluationResultDto> Handle(
        SubmitEvaluationCommand cmd,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(cmd.PracticeSessionId))
            throw new ArgumentException("PracticeSessionId is required.");
        if (string.IsNullOrWhiteSpace(cmd.LearnerId))
            throw new ArgumentException("LearnerId is required.");

        return await _orchestrator.ExecuteEvaluationAsync(cmd, ct);
    }
}
