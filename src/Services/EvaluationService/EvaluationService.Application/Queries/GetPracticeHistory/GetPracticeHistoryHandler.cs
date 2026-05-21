using EvaluationService.Domain.Repositories;
using MediatR;

namespace EvaluationService.Application.Queries.GetPracticeHistory;

public class GetPracticeHistoryHandler
    : IRequestHandler<GetPracticeHistoryQuery, PracticeHistoryResponse>
{
    private readonly IEvaluationRepository _repo;

    public GetPracticeHistoryHandler(IEvaluationRepository repo)
    {
        _repo = repo;
    }

    public async Task<PracticeHistoryResponse> Handle(
        GetPracticeHistoryQuery request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.LearnerId))
            throw new ArgumentException("learnerId is required");

        if (string.IsNullOrWhiteSpace(request.PatientId))
            throw new ArgumentException("patientId is required");

        var rows = await _repo.GetPracticeHistoryAsync(
            request.LearnerId,
            request.PatientId,
            cancellationToken
        );

        return new PracticeHistoryResponse
        {
            LearnerId = request.LearnerId,
            PatientId = request.PatientId,
            Items = rows.Select(r => new PracticeHistoryItemDto
                {
                    PracticeSessionId = r.PracticeSessionId,
                    EvaluationId = r.EvaluationId,
                    Score = r.Score,
                    PureEpaScore = r.PureEpaScore,
                    EntrustmentLevel = r.EntrustmentLevel,
                    FinalDiagnosis = r.FinalDiagnosis,
                    Duration = r.Duration,
                    DiagnosisMatch = r.DiagnosisMatch,
                    RubricVersion = r.RubricVersion,
                    CreatedAt = r.CreatedAt,
                    Status = r.Status,
                    FeedbackId = r.FeedbackId,
                })
                .ToList(),
        };
    }
}
