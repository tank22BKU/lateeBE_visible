using EvaluationService.Application.Dtos;
using EvaluationService.Application.Services;
using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using EvaluationService.Domain.Services;
using Microsoft.Extensions.Logging;
using EvaluationService.Application.Commands.SubmitEvaluation;

namespace EvaluationService.Application.Orchestrators;

///   1. Load session + clinical context từ DB
///   2. Load rubric (từ cache hoặc DB)
///   3. Build prompt (inject rubric + transcripts)
///   4. Call AI (hiện tại sử dụng Gemini, upgrade các model khác so sánh sau --coming soon)
///   5. Aggregate + validate scores (ScoringModifiers)
///   6. Persist (Evaluation + EpaScore rows)
///   7. Return result DTO

public sealed class EvaluationOrchestrator
{
    private readonly IEvaluationRepository           _repo;
    private readonly IRubricProvider                 _rubricProvider;
    private readonly IEvaluationPromptBuilder        _promptBuilder;
    private readonly IAiEvaluationProvider           _geminiRepo;
    private readonly IEpaScoreAggregator             _aggregator;
    private readonly IEvaluationPersistenceService   _persistence;
    private readonly IFeedbackComposer               _feedbackComposer;
    private readonly ILogger<EvaluationOrchestrator> _logger;

    public EvaluationOrchestrator(
        IEvaluationRepository           repo,
        IRubricProvider                 rubricProvider,
        IEvaluationPromptBuilder        promptBuilder,
        IAiEvaluationProvider    geminiRepo,
        IEpaScoreAggregator             aggregator,
        IEvaluationPersistenceService   persistence,
        IFeedbackComposer               feedbackComposer,
        ILogger<EvaluationOrchestrator> logger)
    {
        _repo             = repo;
        _rubricProvider   = rubricProvider;
        _promptBuilder    = promptBuilder;
        _geminiRepo       = geminiRepo;
        _aggregator       = aggregator;
        _persistence      = persistence;
        _feedbackComposer = feedbackComposer;
        _logger           = logger;
    }

    public async Task<SubmitEvaluationResultDto> ExecuteEvaluationAsync(
        SubmitEvaluationCommand cmd,
        CancellationToken       ct = default)
    {
        _logger.LogInformation(
            "Evaluation pipeline start: sessionId={SessionId} learnerId={LearnerId}",
            cmd.PracticeSessionId, cmd.LearnerId);

        var session = await _repo.GetPracticeSessionByIdAsync(cmd.PracticeSessionId)
            ?? throw new InvalidOperationException(
                $"Practice session '{cmd.PracticeSessionId}' not found.");

        session.FinalDiagnosis    = cmd.FinalDiagnosis    ?? session.FinalDiagnosis;
        session.VpConversationLog = cmd.VpConversationLog ?? session.VpConversationLog;
        session.AiReasoningLog    = cmd.AiReasoningLog    ?? session.AiReasoningLog;
        session.DiscussionType    = cmd.DiscussionType    ?? session.DiscussionType;
        session.ModuleId          = cmd.ModuleId          ?? session.ModuleId;
        session.EndTime           = DateTime.UtcNow;
        session.Status            = "Completed";

        var duration = session.EndTime.HasValue
            ? (int)(session.EndTime.Value - session.StartTime).TotalMinutes
            : 0;

        var clinicalDx = await _repo.GetClinicalDiagnosisByPatientIdAsync(session.PatientId);
        var patient    = await _repo.GetVirtualPatientByIdAsync(session.PatientId);

        var rubric = await _rubricProvider.GetRubricAsync(clinicalDx?.EccId ?? string.Empty, ct);

        var input = new EvaluationInput(
            SessionId:                   cmd.PracticeSessionId,
            LearnerId:                   cmd.LearnerId,
            PatientId:                   session.PatientId,
            VpConversationLog:           session.VpConversationLog      ?? string.Empty,
            AiReasoningLog:              session.AiReasoningLog         ?? string.Empty,
            LearnerFinalDiagnosis:       session.FinalDiagnosis         ?? string.Empty,
            CanonicalDiagnosis:          clinicalDx?.CanonicalDiagnosis ?? string.Empty,
            CaseDescription:             clinicalDx?.DescriptionText    ?? string.Empty,
            AllottedVpTimeMinutes:       patient?.TimeSettingMinutes    ?? 30,
            AllottedArgumentTimeMinutes: patient?.ArgumentTimeMinutes   ?? 15,
            ActualDurationMinutes:       duration,
            RubricContent:               rubric.FullContent,
            RubricVersion:               rubric.Version,
            ActiveWarningLabels:         cmd.Warnings.Select(w => w.Label).ToList()
        );

        var prompt = _promptBuilder.Build(input, rubric);

        var aiOutput = await _geminiRepo.AnalyzePerformanceAsync(prompt, ct);

        var aggregated = _aggregator.Aggregate(aiOutput, input);

        var evaluationId = await _persistence.PersistEvaluationAsync(
            practiceSessionId: cmd.PracticeSessionId,
            result:            aggregated,
            rubricVersion:     rubric.Version,
            duration:          duration,
            ct:                ct);

        var persistedEval = await _repo.GetByIdAsync(evaluationId);
        if (persistedEval != null)
        {
            persistedEval.Score    = aggregated.FinalScore;
            persistedEval.Duration = duration;
            await _repo.SaveChangesAsync();
        }
        if (cmd.Warnings.Count > 0)
        {
            var warningEntities = cmd.Warnings.Select(w => new Warning
            {
                Id                = string.IsNullOrEmpty(w.WarningId)
                                        ? Guid.NewGuid().ToString("N")
                                        : w.WarningId,
                PracticeSessionId = cmd.PracticeSessionId,
                LearnerId         = cmd.LearnerId,
                Label             = w.Label,
                Description       = w.Description,
                CreatedAt         = DateTime.UtcNow
            }).ToList();

            await _repo.AddWarningsAsync(warningEntities);
        }

        await _repo.UpdatePracticeSessionAsync(session);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Evaluation complete: evaluationId={EvaluationId} score={Score} level={Level}",
            evaluationId, aggregated.FinalScore, aggregated.OverallEntrustmentLevel);

        return new SubmitEvaluationResultDto
        {
            EvaluationId             = evaluationId,
            PracticeSessionId        = cmd.PracticeSessionId,
            Score                    = aggregated.FinalScore,
            EntrustmentLevel         = aggregated.OverallEntrustmentLevel,
            FeedbackDetail           = aggregated.EvaluationTrace,
            FinalDiagnosis           = session.FinalDiagnosis ?? string.Empty,
            DiagnosisMatchType       = aggregated.DiagnosisMatchType,
            DiagnosisModifier        = aggregated.DiagnosisModifier,
            TimeModifier             = aggregated.TimeModifier,
            WarningPenalty           = aggregated.WarningPenalty,
            WarningCount             = cmd.Warnings.Count,
            SafetyEscalationRequired = aggregated.SafetyEscalationRequired,
            CognitiveAlerts          = aggregated.CognitiveAlerts,
            EpaScores                = aggregated.EpaScores.Select(s => new EpaScoreDto
            {
                EpaId            = s.EpaId,
                NumericalScore   = s.NumericalScore,
                EntrustmentLevel = s.EntrustmentLevel,
                FeedbackDetail   = s.FeedbackDetail ?? string.Empty,
                EvidenceCited    = s.EvidenceCited,
                FailurePatterns  = s.FailurePatterns,
                SafetyFlags      = s.SafetyFlags
            }).ToList(),
            DiscussionType             = session.DiscussionType ?? "Message Type",
            Duration                   = duration,
            PracticeFeedbackAvailable  = false
        };
    }

    public async Task<PracticeFeedbackResponseDto> GenerateFeedbackAsync(
        string            practiceSessionId,
        CancellationToken ct = default)
    {
        var existing = await _repo.GetPracticeFeedbackBySessionIdAsync(practiceSessionId);
        if (existing != null)
        {
            return new PracticeFeedbackResponseDto
            {
                Id             = existing.Id,
                OverallAttempt = existing.OverallAttempt,
                OverallLabel   = existing.OverallLabel,
                Strength       = existing.Strength,
                Improvement    = existing.Improvement,
                CreatedAt      = existing.CreatedAt,
                WasCached      = true
            };
        }
        var session = await _repo.GetPracticeSessionByIdAsync(practiceSessionId)
            ?? throw new InvalidOperationException($"Session '{practiceSessionId}' not found.");

        var evaluations = await _repo.GetByLearnerIdAsync(session.LearnerId);
        var evaluation  = evaluations.FirstOrDefault(e => e.PracticeSessionId == practiceSessionId)
            ?? throw new InvalidOperationException(
                $"No evaluation for session '{practiceSessionId}'. Submit evaluation first.");

        var epaScores = await _repo.GetEpaScoresByEvaluationIdAsync(evaluation.Id);
        var warnings  = await _repo.GetWarningsByPracticeSessionIdAsync(practiceSessionId);

        var feedbackDto = await _feedbackComposer.ComposeAsync(
            session, evaluation, epaScores, warnings, ct);

        var entity = new PracticeFeedback
        {
            Id                = Guid.NewGuid().ToString("N"),
            OverallAttempt    = feedbackDto.OverallAttempt,
            OverallLabel      = feedbackDto.OverallLabel,
            Strength          = feedbackDto.Strength,
            Improvement       = feedbackDto.Improvement,
            CreatedAt         = DateTime.UtcNow,
            UpdatedAt         = DateTime.UtcNow,
            EvaluationId      = evaluation.Id,
            PracticeSessionId = practiceSessionId
        };

        await _repo.AddPracticeFeedbackAsync(entity);
        await _repo.SaveChangesAsync();

        feedbackDto.Id        = entity.Id;
        feedbackDto.CreatedAt = entity.CreatedAt;
        feedbackDto.WasCached = false;
        return feedbackDto;
    }
}