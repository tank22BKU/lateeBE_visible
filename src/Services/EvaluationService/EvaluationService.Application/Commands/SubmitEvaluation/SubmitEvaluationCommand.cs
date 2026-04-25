using MediatR;
using System.Text.Json;
using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using EvaluationService.Application.Dtos;

namespace EvaluationService.Application.Commands.SubmitEvaluation;

public record SubmitEvaluationCommand(
    string SessionId,
    string UserId,
    string ClinicalCaseId,
    string VpLog,
    string ReasoningLog,
    string Diagnosis,
    decimal OverallScore,
    string? CaseType,
    string? DiscussionType,
    string? Duration,
    List<WarningDto> Warnings) : IRequest<SubmitEvaluationResultDto>;

public class SubmitEvaluationHandler : IRequestHandler<SubmitEvaluationCommand, SubmitEvaluationResultDto> {
    private readonly IEvaluationRepository _repo;
    private readonly IGeminiAiRepository _ai;

    public SubmitEvaluationHandler(IEvaluationRepository repo, IGeminiAiRepository ai) {
        _repo = repo; _ai = ai;
    }

    public async Task<SubmitEvaluationResultDto> Handle(SubmitEvaluationCommand request, CancellationToken ct) {
        var result = new EvaluationResult {
            SessionId = request.SessionId,
            UserId = request.UserId,
            ClinicalCaseId = request.ClinicalCaseId,
            CaseType = string.IsNullOrWhiteSpace(request.CaseType) ? "Diagnosis" : request.CaseType,
            DiscussionType = string.IsNullOrWhiteSpace(request.DiscussionType) ? "Message Type" : request.DiscussionType,
            DurationText = string.IsNullOrWhiteSpace(request.Duration) ? "N/A" : request.Duration,
            VpConversationLog = JsonSerializer.Serialize(request.VpLog),
            AiReasoningLog = JsonSerializer.Serialize(request.ReasoningLog),
            FinalDiagnosis = request.Diagnosis,
            OverallScore = request.OverallScore,
            Warnings = request.Warnings.Select(w => new EvaluationWarning
            {
                WarningId = w.WarningId,
                ResultId = string.Empty,
                WarningType = w.Label,
                WarningMessage = w.Description
            }).ToList()
        };

        foreach (var warning in result.Warnings)
        {
            warning.ResultId = result.ResultId;
        }

        result.EpaScores = await _ai.AnalyzePerformanceAsync(result);

        if (result.OverallScore <= 0 && result.EpaScores.Count > 0)
        {
            result.OverallScore = Math.Round(result.EpaScores.Sum(x => x.NumericalScore), 2);
        }

        if (result.OverallScore > 100)
        {
            result.OverallScore = 100;
        }
        
        await _repo.AddAsync(result);
        await _repo.SaveChangesAsync();

        var epaMap = BuildEpaMap(result.EpaScores);

        return new SubmitEvaluationResultDto
        {
            ResultId = result.ResultId,
            FinalScore = result.OverallScore,
            CorrectAnswer = result.FinalDiagnosis ?? string.Empty,
            CaseType = result.CaseType,
            DiscussionType = result.DiscussionType,
            Duration = result.DurationText,
            Evaluation = $"Module {result.ModuleId}",
            DetailedAssessment = epaMap
        };
    }

    private static List<EpaAssessmentItemDto> BuildEpaMap(IEnumerable<EpaScore> scores)
    {
        var titleMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["EPA_1"] = "Information Gathering",
            ["EPA_2"] = "Diagnosis Reasoning & Differential Diagnosis",
            ["EPA_3"] = "Diagnosis Testing",
            ["EPA_4"] = "Management Plan & Safe Order Entry",
            ["EPA_5"] = "Patient Education, Shared Decision-Making & Follow-Up"
        };

        return scores
            .OrderBy(x => x.EpaId)
            .Select(x => new EpaAssessmentItemDto
            {
                EpaId = x.EpaId,
                Title = titleMap.TryGetValue(x.EpaId, out var title) ? title : x.EpaId,
                Score = x.NumericalScore,
                MaxScore = 20,
                Feedback = x.FeedbackDetail
            })
            .ToList();
    }
}

public class SubmitEvaluationResultDto
{
    public string ResultId { get; set; } = string.Empty;
    public decimal FinalScore { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public string DiscussionType { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Evaluation { get; set; } = string.Empty;
    public List<EpaAssessmentItemDto> DetailedAssessment { get; set; } = [];
}

public class EpaAssessmentItemDto
{
    public string EpaId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public string Feedback { get; set; } = string.Empty;
}