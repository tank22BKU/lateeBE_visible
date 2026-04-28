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
        var entity = await _repo.GetByIdAsync(request.ResultId);
        if (entity == null)
        {
            return null;
        }

        var titleMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["EPA_1"] = "Information Gathering",
            ["EPA_2"] = "Diagnosis Reasoning & Differential Diagnosis",
            ["EPA_3"] = "Diagnosis Testing",
            ["EPA_4"] = "Management Plan & Safe Order Entry",
            ["EPA_5"] = "Patient Education, Shared Decision-Making & Follow-Up"
        };

        return new EvaluationReportDto
        {
            ResultId = entity.ResultId,
            SessionId = entity.SessionId,
            UserId = entity.UserId,
            ClinicalCaseId = entity.ClinicalCaseId,
            ModuleId = entity.ModuleId,
            VpConversationLog = entity.VpConversationLog ?? string.Empty,
            AiReasoningLog = entity.AiReasoningLog ?? string.Empty,
            FinalDiagnosis = entity.FinalDiagnosis ?? string.Empty,
            OverallScore = entity.OverallScore,
            FinalScore = entity.OverallScore,
            CorrectAnswer = entity.FinalDiagnosis ?? string.Empty,
            CaseType = entity.CaseType,
            DiscussionType = entity.DiscussionType,
            Duration = entity.DurationText,
            Evaluation = $"Module {entity.ModuleId}",
            CreatedAt = entity.CreatedAt,
            Warnings = entity.Warnings.Select(x => new WarningDto
            {
                WarningId = x.WarningId,
                Label = x.WarningType,
                Description = x.WarningMessage
            }).ToList(),
            EpaScores = entity.EpaScores.Select(x => new EvaluationEpaScoreDto
            {
                ScoreId = x.ScoreId,
                EpaId = x.EpaId,
                Title = titleMap.TryGetValue(x.EpaId, out var title) ? title : x.EpaId,
                EntrustmentLevel = x.EntrustmentLevel,
                NumericalScore = x.NumericalScore,
                MaxScore = 20,
                FeedbackDetail = x.FeedbackDetail
            }).ToList()
        };
    }
}

public class EvaluationReportDto
{
    public string ResultId { get; set; } = default!;
    public string SessionId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string ClinicalCaseId { get; set; } = default!;
    public string ModuleId { get; set; } = default!;
    public string VpConversationLog { get; set; } = default!;
    public string AiReasoningLog { get; set; } = default!;
    public string FinalDiagnosis { get; set; } = default!;
    public decimal OverallScore { get; set; }
    public decimal FinalScore { get; set; }
    public string CorrectAnswer { get; set; } = default!;
    public string CaseType { get; set; } = default!;
    public string DiscussionType { get; set; } = default!;
    public string Duration { get; set; } = default!;
    public string Evaluation { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public List<WarningDto> Warnings { get; set; } = [];
    public List<EvaluationEpaScoreDto> EpaScores { get; set; } = [];
}

public class EvaluationEpaScoreDto
{
    public string ScoreId { get; set; } = default!;
    public string EpaId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public int EntrustmentLevel { get; set; }
    public decimal NumericalScore { get; set; }
    public decimal MaxScore { get; set; }
    public string FeedbackDetail { get; set; } = default!;
}