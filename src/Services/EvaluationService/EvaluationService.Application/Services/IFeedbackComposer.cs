using EvaluationService.Application.Dtos;
using EvaluationService.Domain.Entities;

namespace EvaluationService.Application.Services;

public interface IFeedbackComposer
{
    Task<PracticeFeedbackResponseDto> ComposeAsync(
        PracticeSession session,
        Evaluation evaluation,
        List<EvaluationEpaScore> epaScores,
        List<Warning> warnings,
        CancellationToken ct = default
    );
}
