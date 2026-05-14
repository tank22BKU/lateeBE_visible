using MediatR;
using EvaluationService.Application.Dtos;
using EvaluationService.Application.Orchestrators;

namespace EvaluationService.Application.Commands.GeneratePracticeFeedback;

public record GeneratePracticeFeedbackCommand(
    string PracticeSessionId
) : IRequest<PracticeFeedbackResponseDto>;

public class GeneratePracticeFeedbackHandler
    : IRequestHandler<GeneratePracticeFeedbackCommand, PracticeFeedbackResponseDto>
{
    private readonly EvaluationOrchestrator _orchestrator;

    public GeneratePracticeFeedbackHandler(EvaluationOrchestrator orchestrator)
        => _orchestrator = orchestrator;

    public async Task<PracticeFeedbackResponseDto> Handle(
        GeneratePracticeFeedbackCommand request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.PracticeSessionId))
            throw new ArgumentException("PracticeSessionId is required.");

        return await _orchestrator.GenerateFeedbackAsync(request.PracticeSessionId, ct);
    }
}