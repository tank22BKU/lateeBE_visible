using MediatR;
using EvaluationService.Application.Dtos;
using EvaluationService.Domain.Repositories;

namespace EvaluationService.Application.Queries.GetReport;

public record GetEvaluationReportQuery(string ResultId) : IRequest<EvaluationReportDto?>;

public class GetEvaluationReportHandler : IRequestHandler<GetEvaluationReportQuery, EvaluationReportDto?>
{
    private readonly IEvaluationRepository _repo;

    public GetEvaluationReportHandler(IEvaluationRepository repo)
    {
        _repo = repo;
    }

    public async Task<EvaluationReportDto?> Handle(GetEvaluationReportQuery request, CancellationToken cancellationToken)
    {
        var evaluation = await _repo.GetByIdAsync(request.ResultId);
        if (evaluation == null)
        {
            return null;
        }

        var session = await _repo.GetPracticeSessionByIdAsync(evaluation.PracticeSessionId);
        if (session == null)
        {
            return null;
        }

        var warnings = await _repo.GetWarningsByPracticeSessionIdAsync(evaluation.PracticeSessionId);

        return new EvaluationReportDto
        {
            EvaluationId = evaluation.Id,
            EpaId = evaluation.EpaId,
            PracticeSessionId = evaluation.PracticeSessionId,
            LearnerId = session.LearnerId,
            PatientId = session.PatientId,
            ModuleId = session.ModuleId ?? string.Empty,
            DiscussionType = session.DiscussionType ?? "Message Type",
            FinalDiagnosis = session.FinalDiagnosis ?? string.Empty,
            VpConversationLog = session.VpConversationLog ?? string.Empty,
            AiReasoningLog = session.AiReasoningLog ?? string.Empty,
            Score = evaluation.Score,
            Duration = evaluation.Duration,
            FeedbackDetail = evaluation.FeedbackDetail,
            EntrustmentLevel = evaluation.EntrustmentLevel,
            CreatedAt = evaluation.CreatedAt,
            Warnings = warnings.Select(x => new WarningDto
            {
                WarningId = x.Id,
                Label = x.Label ?? string.Empty,
                Description = x.Description ?? string.Empty
            }).ToList()
        };
    }
}

public class EvaluationReportDto
{
    public string EvaluationId { get; set; } = default!;
    public string EpaId { get; set; } = default!;
    public string PracticeSessionId { get; set; } = default!;
    public string LearnerId { get; set; } = default!;
    public string PatientId { get; set; } = default!;
    public string ModuleId { get; set; } = default!;
    public string DiscussionType { get; set; } = default!;
    public string FinalDiagnosis { get; set; } = default!;
    public string VpConversationLog { get; set; } = default!;
    public string AiReasoningLog { get; set; } = default!;
    public decimal? Score { get; set; }
    public int? Duration { get; set; }
    public string? FeedbackDetail { get; set; }
    public int? EntrustmentLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<WarningDto> Warnings { get; set; } = [];
}
