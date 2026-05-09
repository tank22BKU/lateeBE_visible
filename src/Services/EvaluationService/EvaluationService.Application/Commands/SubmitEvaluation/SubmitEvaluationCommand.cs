using MediatR;
using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using EvaluationService.Application.Dtos;

namespace EvaluationService.Application.Commands.SubmitEvaluation;

public record SubmitEvaluationCommand(
    string PracticeSessionId,
    string LearnerId,
    string EpaId,
    decimal? Score,
    int? Duration,
    string? FeedbackDetail,
    int? EntrustmentLevel,
    string? FinalDiagnosis,
    string? VpConversationLog,
    string? AiReasoningLog,
    string? DiscussionType,
    string? ModuleId,
    List<WarningDto> Warnings) : IRequest<SubmitEvaluationResultDto>;

public class SubmitEvaluationHandler : IRequestHandler<SubmitEvaluationCommand, SubmitEvaluationResultDto> {
    private readonly IEvaluationRepository _repo;

    public SubmitEvaluationHandler(IEvaluationRepository repo) {
        _repo = repo;
    }

    public async Task<SubmitEvaluationResultDto> Handle(SubmitEvaluationCommand request, CancellationToken ct) {
        var session = await _repo.GetPracticeSessionByIdAsync(request.PracticeSessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Practice session không tồn tại.");
        }

        session.FinalDiagnosis = request.FinalDiagnosis ?? session.FinalDiagnosis;
        session.VpConversationLog = request.VpConversationLog ?? session.VpConversationLog;
        session.AiReasoningLog = request.AiReasoningLog ?? session.AiReasoningLog;
        session.DiscussionType = request.DiscussionType ?? session.DiscussionType;
        session.ModuleId = request.ModuleId ?? session.ModuleId;
        session.EndTime = DateTime.UtcNow;
        session.Status = "Completed";

        var evaluation = new Evaluation
        {
            Id = Guid.NewGuid().ToString("N"),
            EpaId = request.EpaId,
            PracticeSessionId = request.PracticeSessionId,
            Score = request.Score,
            Duration = request.Duration,
            FeedbackDetail = request.FeedbackDetail,
            EntrustmentLevel = request.EntrustmentLevel,
            CreatedAt = DateTime.UtcNow
        };

        var warnings = request.Warnings.Select(w => new Warning
        {
            Id = w.WarningId,
            PracticeSessionId = request.PracticeSessionId,
            LearnerId = request.LearnerId,
            Label = w.Label,
            Description = w.Description,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await _repo.AddEvaluationAsync(evaluation);
        await _repo.AddWarningsAsync(warnings);
        await _repo.UpdatePracticeSessionAsync(session);
        await _repo.SaveChangesAsync();

        return new SubmitEvaluationResultDto
        {
            EvaluationId = evaluation.Id,
            PracticeSessionId = evaluation.PracticeSessionId,
            Score = evaluation.Score,
            EntrustmentLevel = evaluation.EntrustmentLevel,
            FeedbackDetail = evaluation.FeedbackDetail,
            FinalDiagnosis = session.FinalDiagnosis ?? string.Empty,
            DiscussionType = session.DiscussionType ?? "Message Type",
            Duration = evaluation.Duration
        };
    }
}

public class SubmitEvaluationResultDto
{
    public string EvaluationId { get; set; } = string.Empty;
    public string PracticeSessionId { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public int? EntrustmentLevel { get; set; }
    public string? FeedbackDetail { get; set; }
    public string FinalDiagnosis { get; set; } = string.Empty;
    public string DiscussionType { get; set; } = string.Empty;
    public int? Duration { get; set; }
}