src\Services\EvaluationService\EvaluationService.API\Controllers\EvaluationController.cs:"using EvaluationService.Application.Commands.CreateIssue;

using EvaluationService.Application.Commands.DeleteEvaluation;

using EvaluationService.Application.Commands.GeneratePracticeFeedback;

using EvaluationService.Application.Commands.SubmitEvaluation;

using EvaluationService.Application.Queries.GetHistory;

using EvaluationService.Application.Queries.GetIssues;

using EvaluationService.Application.Queries.GetPracticeHistory;

using EvaluationService.Application.Queries.GetReport;

using MediatR;

using Microsoft.AspNetCore.Mvc;



namespace EvaluationService.API.Controllers;



[ApiController]

[Route("api/evaluation")]

public class EvaluationController : ControllerBase

{

    private readonly IMediator _mediator;



    public EvaluationController(IMediator mediator) => _mediator = mediator;



    [HttpPost("submit")]

    public async Task<IActionResult> Submit([FromBody] SubmitEvaluationCommand cmd)

    {

        var result = await _mediator.Send(cmd);

        return Ok(new { message = "Evaluation saved successfully.", data = result });

    }



    [HttpGet("{userId}/history")]

    public async Task<IActionResult> GetHistory(string userId) =>

        Ok(await _mediator.Send(new GetUserHistoryQuery(userId)));



    [HttpGet("{id}/report")]

    public async Task<IActionResult> GetReport(string id)

    {

        var res = await _mediator.Send(new GetEvaluationReportQuery(id));

        return res != null ? Ok(res) : NotFound(new { message = $"Evaluation '{id}' not found." });

    }



    [HttpPost("practice-feedback/{practiceSessionId}")]

    public async Task<IActionResult> GeneratePracticeFeedback(string practiceSessionId)

    {

        var result = await _mediator.Send(new GeneratePracticeFeedbackCommand(practiceSessionId));

        return Ok(

            new

            {

                message = result.WasCached

                    ? "Feedback retrieved from cache."

                    : "Feedback generated successfully.",

                data = result,

            }

        );

    }



    [HttpDelete("{id}")]

    public async Task<IActionResult> Delete(string id)

    {

        var deleted = await _mediator.Send(new DeleteEvaluationCommand(id));

        return deleted ? NoContent() : NotFound(new { message = $"Evaluation '{id}' not found." });

    }



    [HttpGet("issues")]

    public async Task<IActionResult> GetIssues(

        [FromQuery] string practiceSessionId,

        [FromQuery] string learnerId

    )

    {

        var result = await _mediator.Send(new GetIssuesQuery(practiceSessionId, learnerId));

        return Ok(result);

    }



    [HttpPost("issues")]

    public async Task<IActionResult> CreateIssue([FromBody] CreateIssueCommand cmd)

    {

        var result = await _mediator.Send(cmd);

        return Ok(new { message = "Issue created successfully.", data = result });

    }



    [HttpGet("practice-history")]

    public async Task<IActionResult> GetPracticeHistory(

        [FromQuery] string learnerId,

        [FromQuery] string patientId

    )

    {

        try

        {

            var result = await _mediator.Send(

                new GetPracticeHistoryQuery { LearnerId = learnerId, PatientId = patientId }

            );



            return Ok(result);

        }

        catch (ArgumentException ex)

        {

            return BadRequest(new { message = ex.Message });

        }

    }

}

" src\Services\EvaluationService\EvaluationService.API\Program.cs:"using EvaluationService.Application;

using EvaluationService.Infrastructure;

using EvaluationService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);



var connectionString =

    builder.Configuration.GetConnectionString("EvaluationDb")

    ?? builder.Configuration.GetConnectionString("DefaultConnection");



builder.Services.AddDbContext<EvaluationDbContext>(options =>

{

    options.UseMySql(

        connectionString,

        ServerVersion.AutoDetect(connectionString)

    );

});



builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);



builder.Services.AddControllers();



builder.Services.AddCors(options =>

{

    options.AddPolicy("SwaggerCors", policy =>

    {

        policy.AllowAnyOrigin()

            .AllowAnyMethod()

            .AllowAnyHeader();

    });

});



builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();



var app = builder.Build();



app.UseCors("SwaggerCors");



if (app.Environment.IsDevelopment())

{

    app.UseSwagger();

    app.UseSwaggerUI(c =>

    {

        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Evaluation API v1");

    });

}



app.UseAuthorization();

app.MapControllers();



app.Run();

" src\Services\EvaluationService\EvaluationService.Application\Commands\CreateIssue\CreateIssueCommand.cs:"using EvaluationService.Application.Dtos;

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

" src\Services\EvaluationService\EvaluationService.Application\Dtos\ClinicalCaseDiagnosisDto.cs:"namespace EvaluationService.Application.Dtos;



public record ClinicalCaseDiagnosisDto(

    string CaseId,

    string CanonicalDiagnosis, // clinical_case.type

    string DescriptionText, // clinical_case.description

    string Symptom,

    string MedicalHistory

);

" src\Services\EvaluationService\EvaluationService.Application\Dtos\EpaScoreDto.cs:"namespace EvaluationService.Application.Dtos;



public class EpaScoreDto

{

    public string EpaId { get; set; } = string.Empty;

    public int NumericalScore { get; set; }

    public int MaxScore { get; set; } = 20;

    public int EntrustmentLevel { get; set; }

    public string FeedbackDetail { get; set; } = string.Empty;

    public List<string> EvidenceCited { get; set; } = [];

    public List<string> FailurePatterns { get; set; } = [];

    public List<string> SafetyFlags { get; set; } = [];

}

" src\Services\EvaluationService\EvaluationService.Application\Dtos\EvaluationReportDto.cs:"namespace EvaluationService.Application.Dtos;



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

    public string? EvaluationTrace { get; set; }

    public int? EntrustmentLevel { get; set; }

    public string? RubricVersion { get; set; }

    public int PureEpaScore { get; set; }

    public int PositiveAdjustmentTotal { get; set; }

    public int NegativeAdjustmentTotal { get; set; }

    public int AdjustmentTotal { get; set; }



    public DiagnosisMatchDto DiagnosisMatch { get; set; } = new();

    public string DiagnosisMatchType { get; set; } = string.Empty;

    public int DiagnosisModifier { get; set; }

    public int TimeModifier { get; set; }

    public int WarningPenalty { get; set; }

    public bool SafetyEscalationRequired { get; set; }

    public List<string> CognitiveAlerts { get; set; } = [];

    public List<EpaScoreDto> EpaScores { get; set; } = [];

    public ScoringAdjustmentsDto Adjustments { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public List<WarningDto> Warnings { get; set; } = [];

    public PracticeFeedbackDto? PracticeFeedback { get; set; }

}



public class PracticeFeedbackDto

{

    public string Id { get; set; } = default!;

    public string? OverallAttempt { get; set; }

    public string? OverallLabel { get; set; }

    public string? Strength { get; set; }

    public string? Improvement { get; set; }

    public DateTime CreatedAt { get; set; }

}

" src\Services\EvaluationService\EvaluationService.Application\Dtos\IssueListResponseDto.cs:"namespace EvaluationService.Application.Dtos;



public class IssueListResponseDto

{

    public List<IssueItemDto> Items { get; set; } = [];

}



public class IssueItemDto

{

    public string IssueId { get; set; } = string.Empty;



    public string LearnerId { get; set; } = string.Empty;



    public string LearnerName { get; set; } = string.Empty;



    public DateTime CreatedAt { get; set; }



    public string? Label { get; set; }



    public string Description { get; set; } = string.Empty;



    public string Status { get; set; } = "Open";



    public IssueExpertFeedbackDto? ExpertFeedback { get; set; }

}



public class IssueExpertFeedbackDto

{

    public string ExpertId { get; set; } = string.Empty;



    public string ExpertName { get; set; } = string.Empty;



    public string Feedback { get; set; } = string.Empty;

}



public class CreateIssueResultDto

{

    public string IssueId { get; set; } = string.Empty;



    public DateTime CreatedAt { get; set; }



    public string Status { get; set; } = "Open";

}

" src\Services\EvaluationService\EvaluationService.Application\Dtos\PracticeFeedbackResponseDto.cs:"namespace EvaluationService.Application.Dtos;



public class PracticeFeedbackResponseDto

{

    public string Id { get; set; } = string.Empty;

    public string? OverallAttempt { get; set; }

    public string? OverallLabel { get; set; }

    public string? Strength { get; set; }

    public string? Improvement { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool WasCached { get; set; }

}

" src\Services\EvaluationService\EvaluationService.Application\Orchestrators\EvaluationOrchestrator.cs:"using EvaluationService.Application.Commands.SubmitEvaluation;

using EvaluationService.Application.Dtos;

using EvaluationService.Application.Services;

using EvaluationService.Domain.Entities;

using EvaluationService.Domain.Repositories;

using EvaluationService.Domain.Services;

using EvaluationService.Domain.ValueObjects;

using Microsoft.Extensions.Logging;



namespace EvaluationService.Application.Orchestrators;



public sealed class EvaluationOrchestrator

{

    private readonly IEvaluationRepository _repo;

    private readonly IRubricProvider _rubricProvider;

    private readonly IEvaluationPromptBuilder _promptBuilder;

    private readonly IAiEvaluationProvider _geminiRepo;

    private readonly IEpaScoreAggregator _aggregator;

    private readonly IEvaluationPersistenceService _persistence;

    private readonly IFeedbackComposer _feedbackComposer;

    private readonly ILogger<EvaluationOrchestrator> _logger;



    public EvaluationOrchestrator(

        IEvaluationRepository repo,

        IRubricProvider rubricProvider,

        IEvaluationPromptBuilder promptBuilder,

        IAiEvaluationProvider geminiRepo,

        IEpaScoreAggregator aggregator,

        IEvaluationPersistenceService persistence,

        IFeedbackComposer feedbackComposer,

        ILogger<EvaluationOrchestrator> logger

    )

    {

        _repo = repo;

        _rubricProvider = rubricProvider;

        _promptBuilder = promptBuilder;

        _geminiRepo = geminiRepo;

        _aggregator = aggregator;

        _persistence = persistence;

        _feedbackComposer = feedbackComposer;

        _logger = logger;

    }



    public async Task<SubmitEvaluationResultDto> ExecuteEvaluationAsync(

        SubmitEvaluationCommand cmd,

        CancellationToken ct = default

    )

    {

        _logger.LogInformation(

            "Evaluation pipeline start: sessionId={SessionId} learnerId={LearnerId}",

            cmd.PracticeSessionId,

            cmd.LearnerId

        );



        var session =

            await _repo.GetPracticeSessionByIdAsync(cmd.PracticeSessionId)

            ?? throw new InvalidOperationException(

                $"Practice session '{cmd.PracticeSessionId}' not found."

            );



        session.FinalDiagnosis = cmd.FinalDiagnosis ?? session.FinalDiagnosis;

        session.VpConversationLog = cmd.VpConversationLog ?? session.VpConversationLog;

        session.AiReasoningLog = cmd.AiReasoningLog ?? session.AiReasoningLog;

        session.DiscussionType = cmd.DiscussionType ?? session.DiscussionType;

        session.ModuleId = cmd.ModuleId ?? session.ModuleId;

        session.EndTime = DateTime.UtcNow;

        session.Status = "Completed";



        var duration = session.EndTime.HasValue

            ? (int)(session.EndTime.Value - session.StartTime).TotalMinutes

            : 0;



        var clinicalDx = await _repo.GetClinicalDiagnosisByPatientIdAsync(session.PatientId);

        var patient = await _repo.GetVirtualPatientByIdAsync(session.PatientId);

        var rubric = await _rubricProvider.GetRubricAsync(clinicalDx?.EccId ?? string.Empty, ct);



        var input = new EvaluationInput(

            SessionId: cmd.PracticeSessionId,

            LearnerId: cmd.LearnerId,

            PatientId: session.PatientId,

            VpConversationLog: session.VpConversationLog ?? string.Empty,

            AiReasoningLog: session.AiReasoningLog ?? string.Empty,

            LearnerFinalDiagnosis: session.FinalDiagnosis ?? string.Empty,

            CanonicalDiagnosis: clinicalDx?.CanonicalDiagnosis ?? string.Empty,

            CaseDescription: clinicalDx?.DescriptionText ?? string.Empty,

            AllottedVpTimeMinutes: patient?.TimeSettingMinutes ?? 30,

            AllottedArgumentTimeMinutes: patient?.ArgumentTimeMinutes ?? 15,

            ActualDurationMinutes: duration,

            RubricContent: rubric.FullContent,

            RubricVersion: rubric.Version,

            ActiveWarningLabels: cmd.Warnings.Select(w => w.Label).ToList(),

            ActiveWarningDescriptions: cmd.Warnings.Select(w => w.Description).ToList()

        );



        var prompt = _promptBuilder.Build(input, rubric);

        var aiOutput = await _geminiRepo.AnalyzePerformanceAsync(prompt, ct);

        var aggregated = _aggregator.Aggregate(aiOutput, input);



        var evaluationId = await _persistence.PersistEvaluationAsync(

            practiceSessionId: cmd.PracticeSessionId,

            result: aggregated,

            rubricVersion: rubric.Version,

            duration: duration,

            ct: ct

        );



        var persistedEval = await _repo.GetByIdAsync(evaluationId);

        if (persistedEval != null)

        {

            persistedEval.Score = aggregated.FinalScore;

            persistedEval.Duration = duration;

            persistedEval.PureEpaScore = aggregated.PureEpaScore;

            await _repo.SaveChangesAsync();

        }



        if (cmd.Warnings.Count > 0)

        {

            var warningEntities = cmd

                .Warnings.Select(w => new Warning

                {

                    Id = string.IsNullOrEmpty(w.WarningId)

                        ? Guid.NewGuid().ToString("N")

                        : w.WarningId,

                    PracticeSessionId = cmd.PracticeSessionId,

                    LearnerId = cmd.LearnerId,

                    Label = w.Label,

                    Description = w.Description,

                    CreatedAt = DateTime.UtcNow,

                })

                .ToList();



            await _repo.AddWarningsAsync(warningEntities);

        }



        await _repo.UpdatePracticeSessionAsync(session);

        await _repo.SaveChangesAsync();



        _logger.LogInformation(

            "Evaluation complete: evaluationId={EvaluationId} finalScore={Score} "

                + "pureEpa={PureEpa} adjTotal={AdjTotal} level={Level}",

            evaluationId,

            aggregated.FinalScore,

            aggregated.PureEpaScore,

            aggregated.AdjustmentTotal,

            aggregated.OverallEntrustmentLevel

        );



        return BuildSubmitResult(aggregated, evaluationId, cmd, session, duration);

    }



    private static SubmitEvaluationResultDto BuildSubmitResult(

        AggregatedEvaluationResult aggregated,

        string evaluationId,

        SubmitEvaluationCommand cmd,

        Domain.Entities.PracticeSession session,

        int duration

    )

    {

        var diagResult = DiagnosisMatchResult.From(aggregated.DiagnosisMatchType);

        var diagAdj = aggregated

            .Adjustments.Positive.Concat(aggregated.Adjustments.Negative)

            .FirstOrDefault(a => a.Source == "diagnosis");

        var timeAdj = aggregated

            .Adjustments.Positive.Concat(aggregated.Adjustments.Negative)

            .FirstOrDefault(a => a.Source == "time");

        var warningNegTotal = aggregated

            .Adjustments.Negative.Where(a => a.Source == "warning")

            .Sum(a => Math.Abs(a.Score));



        return new SubmitEvaluationResultDto

        {

            EvaluationId = evaluationId,

            PracticeSessionId = cmd.PracticeSessionId,

            Score = aggregated.FinalScore,

            EntrustmentLevel = aggregated.OverallEntrustmentLevel,

            FeedbackDetail = aggregated.EvaluationTrace,

            FinalDiagnosis = session.FinalDiagnosis ?? string.Empty,



            PureEpaScore = aggregated.PureEpaScore,

            PositiveAdjustmentTotal = aggregated.PositiveAdjustmentTotal,

            NegativeAdjustmentTotal = aggregated.NegativeAdjustmentTotal,

            AdjustmentTotal = aggregated.AdjustmentTotal,



            DiagnosisMatch = new DiagnosisMatchDto

            {

                MatchType = diagResult.MatchType,

                MatchTypeLabel = diagResult.MatchTypeLabel,

                IsAcceptable = diagResult.IsAcceptable,

                IsDangerous = diagResult.IsDangerous,

                RequiresSafetyReview = diagResult.RequiresSafetyReview,

            },



            DiagnosisMatchType = aggregated.DiagnosisMatchType,

            DiagnosisModifier = diagAdj?.Score ?? 0,

            TimeModifier = timeAdj?.Score ?? 0,

            WarningPenalty = warningNegTotal,

            WarningCount = cmd.Warnings.Count,

            SafetyEscalationRequired = aggregated.SafetyEscalationRequired,

            CognitiveAlerts = aggregated.CognitiveAlerts,



            EpaScores = aggregated

                .EpaScores.Select(s => new EpaScoreDto

                {

                    EpaId = s.EpaId,

                    NumericalScore = s.NumericalScore,

                    MaxScore = 20,

                    EntrustmentLevel = s.EntrustmentLevel,

                    FeedbackDetail = s.FeedbackDetail ?? string.Empty,

                    EvidenceCited = s.EvidenceCited,

                    FailurePatterns = s.FailurePatterns,

                    SafetyFlags = s.SafetyFlags,

                })

                .ToList(),



            Adjustments = MapAdjustmentsDto(aggregated.Adjustments),



            DiscussionType = session.DiscussionType ?? "Message Type",

            Duration = duration,

            PracticeFeedbackAvailable = false,

        };

    }



    public async Task<PracticeFeedbackResponseDto> GenerateFeedbackAsync(

        string practiceSessionId,

        CancellationToken ct = default

    )

    {

        var existing = await _repo.GetPracticeFeedbackBySessionIdAsync(practiceSessionId);

        if (existing != null)

            return new PracticeFeedbackResponseDto

            {

                Id = existing.Id,

                OverallAttempt = existing.OverallAttempt,

                OverallLabel = existing.OverallLabel,

                Strength = existing.Strength,

                Improvement = existing.Improvement,

                CreatedAt = existing.CreatedAt,

                WasCached = true,

            };



        var session =

            await _repo.GetPracticeSessionByIdAsync(practiceSessionId)

            ?? throw new InvalidOperationException($"Session '{practiceSessionId}' not found.");



        var evaluations = await _repo.GetByLearnerIdAsync(session.LearnerId);

        var evaluation =

            evaluations.FirstOrDefault(e => e.PracticeSessionId == practiceSessionId)

            ?? throw new InvalidOperationException(

                $"No evaluation for session '{practiceSessionId}'. Submit evaluation first."

            );



        var epaScores = await _repo.GetEpaScoresByEvaluationIdAsync(evaluation.Id);

        var warnings = await _repo.GetWarningsByPracticeSessionIdAsync(practiceSessionId);



        var feedbackDto = await _feedbackComposer.ComposeAsync(

            session,

            evaluation,

            epaScores,

            warnings,

            ct

        );



        var entity = new PracticeFeedback

        {

            Id = Guid.NewGuid().ToString("N"),

            OverallAttempt = feedbackDto.OverallAttempt,

            OverallLabel = feedbackDto.OverallLabel,

            Strength = feedbackDto.Strength,

            Improvement = feedbackDto.Improvement,

            CreatedAt = DateTime.UtcNow,

            UpdatedAt = DateTime.UtcNow,

            EvaluationId = evaluation.Id,

            PracticeSessionId = practiceSessionId,

        };



        await _repo.AddPracticeFeedbackAsync(entity);

        await _repo.SaveChangesAsync();



        feedbackDto.Id = entity.Id;

        feedbackDto.CreatedAt = entity.CreatedAt;

        feedbackDto.WasCached = false;

        return feedbackDto;

    }



    private static ScoringAdjustmentsDto MapAdjustmentsDto(ScoringAdjustments adj) =>

        new()

        {

            Positive = adj

                .Positive.Select(a => new ScoringAdjustmentDto

                {

                    Code = a.Code,

                    Title = a.Title,

                    Score = a.Score,

                    Reason = a.Reason,

                    Source = a.Source,

                    Severity = a.Severity,

                })

                .ToList(),

            Negative = adj

                .Negative.Select(a => new ScoringAdjustmentDto

                {

                    Code = a.Code,

                    Title = a.Title,

                    Score = a.Score,

                    Reason = a.Reason,

                    Source = a.Source,

                    Severity = a.Severity,

                })

                .ToList(),

            Validation = new ValidationSummaryDto

            {

                HasEthicsViolation = adj.Validation.HasEthicsViolation,

                HasUnsafeQuestion = adj.Validation.HasUnsafeQuestion,

                HasWorkflowViolation = adj.Validation.HasWorkflowViolation,

                SafetyEscalationRequired = adj.Validation.SafetyEscalationRequired,

                TotalWarnings = adj.Validation.TotalWarnings,

            },

        };

}

" src\Services\EvaluationService\EvaluationService.Application\Orchestrators\PracticeFeedbackOrchestrator.cs:"using System.Text.Json;

using EvaluationService.Application.Dtos;

using EvaluationService.Application.Services;

using EvaluationService.Domain.Entities;

using EvaluationService.Domain.Repositories;

using Microsoft.Extensions.Logging;



namespace EvaluationService.Application.Orchestrators;



public sealed class PracticeFeedbackOrchestrator

{

    private readonly IEvaluationRepository _repo;

    private readonly IFeedbackComposer _feedbackComposer;

    private readonly ILogger<PracticeFeedbackOrchestrator> _logger;



    public PracticeFeedbackOrchestrator(

        IEvaluationRepository repo,

        IFeedbackComposer feedbackComposer,

        ILogger<PracticeFeedbackOrchestrator> logger

    )

    {

        _repo = repo;

        _feedbackComposer = feedbackComposer;

        _logger = logger;

    }



    public async Task<PracticeFeedbackResponseDto> GenerateFeedbackAsync(

        string practiceSessionId,

        CancellationToken ct = default

    )

    {

        var existing = await _repo.GetPracticeFeedbackBySessionIdAsync(practiceSessionId);

        if (existing != null && !IsFallbackFeedback(existing))

            return new PracticeFeedbackResponseDto

            {

                Id = existing.Id,

                OverallAttempt = existing.OverallAttempt,

                OverallLabel = existing.OverallLabel,

                Strength = NormalizeStrength(existing.Strength),

                Improvement = NormalizeImprovement(existing.Improvement),

                CreatedAt = existing.CreatedAt,

                WasCached = true,

            };



        var session =

            await _repo.GetPracticeSessionByIdAsync(practiceSessionId)

            ?? throw new InvalidOperationException($"Session '{practiceSessionId}' not found.");



        var clinicalDx = await _repo.GetClinicalDiagnosisByPatientIdAsync(session.PatientId);

        var patient = await _repo.GetVirtualPatientByIdAsync(session.PatientId);



        var evaluations = await _repo.GetByLearnerIdAsync(session.LearnerId);

        var evaluation =

            evaluations.FirstOrDefault(e => e.PracticeSessionId == practiceSessionId)

            ?? throw new InvalidOperationException(

                $"No evaluation for session '{practiceSessionId}'. Submit evaluation first."

            );



        var epaScores = await _repo.GetEpaScoresByEvaluationIdAsync(evaluation.Id);

        var warnings = await _repo.GetWarningsByPracticeSessionIdAsync(practiceSessionId);



        _logger.LogInformation(

            "Practice feedback pipeline start: sessionId={SessionId} evaluationId={EvaluationId}",

            practiceSessionId,

            evaluation.Id

        );



        var feedbackDto = await _feedbackComposer.ComposeAsync(

            session,

            evaluation,

            epaScores,

            warnings,

            ct

        );



        var entity = new PracticeFeedback

        {

            Id = Guid.NewGuid().ToString("N"),

            OverallAttempt = feedbackDto.OverallAttempt,

            OverallLabel = feedbackDto.OverallLabel,

            Strength = feedbackDto.Strength,

            Improvement = feedbackDto.Improvement,

            CreatedAt = DateTime.UtcNow,

            UpdatedAt = DateTime.UtcNow,

            EvaluationId = evaluation.Id,

            PracticeSessionId = practiceSessionId,

        };



        await _repo.AddPracticeFeedbackAsync(entity);

        await _repo.SaveChangesAsync();



        feedbackDto.Id = entity.Id;

        feedbackDto.CreatedAt = entity.CreatedAt;

        feedbackDto.WasCached = false;

        return feedbackDto;

    }



    private static bool IsFallbackFeedback(PracticeFeedback feedback)

    {

        var strength = feedback.Strength ?? string.Empty;

        var improvement = feedback.Improvement ?? string.Empty;

        var overallAttempt = feedback.OverallAttempt ?? string.Empty;



        return strength.Contains("Feedback requires AI service", StringComparison.OrdinalIgnoreCase)

            || improvement.Contains(

                "Review your EPA performance breakdown above for specific areas.",

                StringComparison.OrdinalIgnoreCase

            )

            || overallAttempt.Contains(

                "Detailed coaching unavailable.",

                StringComparison.OrdinalIgnoreCase

            );

    }



    private static string? NormalizeStrength(string? strength)

    {

        if (string.IsNullOrWhiteSpace(strength))

            return null;



        var text = strength.Trim();



        if (TryParseJsonPayload(text, out var normalized))

            return normalized;



        var cleaned = string.Join(

            " ",

            text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)

                .Select(line => line.Trim())

                .Where(line =>

                    !string.Equals(line, "Clinical Strengths", StringComparison.OrdinalIgnoreCase)

                    && !string.Equals(line, "Very good", StringComparison.OrdinalIgnoreCase)

                )

        );



        return string.IsNullOrWhiteSpace(cleaned) ? text : CollapseWhitespace(cleaned);

    }



    private static string? NormalizeImprovement(string? improvement)

    {

        if (string.IsNullOrWhiteSpace(improvement))

            return null;



        return CollapseWhitespace(improvement);

    }



    private static bool TryParseJsonPayload(string text, out string normalized)

    {

        normalized = string.Empty;



        var candidate = ExtractJsonCandidate(text);

        if (string.IsNullOrWhiteSpace(candidate))

            return false;



        try

        {

            using var doc = JsonDocument.Parse(candidate);

            normalized = doc.RootElement.ValueKind switch

            {

                JsonValueKind.String => CollapseWhitespace(

                    doc.RootElement.GetString() ?? string.Empty

                ),

                JsonValueKind.Array => string.Join(

                    " ",

                    doc.RootElement.EnumerateArray()

                        .Select(NormalizeJsonItem)

                        .Where(value => !string.IsNullOrWhiteSpace(value))

                ),

                JsonValueKind.Object => NormalizeJsonObject(doc.RootElement),

                _ => CollapseWhitespace(doc.RootElement.ToString()),

            };



            return !string.IsNullOrWhiteSpace(normalized);

        }

        catch (JsonException)

        {

            return false;

        }

    }



    private static string NormalizeJsonItem(JsonElement item) =>

        item.ValueKind switch

        {

            JsonValueKind.String => item.GetString() ?? string.Empty,

            JsonValueKind.Object => NormalizeJsonObject(item),

            _ => item.ToString(),

        };



    private static string NormalizeJsonObject(JsonElement item)

    {

        var what = ReadString(item, "what");

        var evidence = ReadString(item, "evidence");

        var whyItMattered = ReadString(item, "why_it_mattered_clinically");



        var parts = new[] { what, evidence, whyItMattered }

            .Where(value => !string.IsNullOrWhiteSpace(value))

            .Select(value => value!.Trim())

            .ToList();



        return parts.Count > 0 ? CollapseWhitespace(string.Join(" ", parts)) : item.ToString();

    }



    private static string? ReadString(JsonElement root, string propertyName)

    {

        if (!root.TryGetProperty(propertyName, out var element))

            return null;



        return element.ValueKind switch

        {

            JsonValueKind.String => element.GetString(),

            JsonValueKind.Number => element.ToString(),

            JsonValueKind.True => "true",

            JsonValueKind.False => "false",

            _ => element.ToString(),

        };

    }



    private static string ExtractJsonCandidate(string text)

    {

        var firstBracket = text.IndexOf('[');

        var firstBrace = text.IndexOf('{');



        var start =

            firstBracket >= 0 && firstBrace >= 0

                ? Math.Min(firstBracket, firstBrace)

                : Math.Max(firstBracket, firstBrace);



        if (start < 0)

            return string.Empty;



        var endBracket = text.LastIndexOf(']');

        var endBrace = text.LastIndexOf('}');

        var end = Math.Max(endBracket, endBrace);



        if (end <= start)

            return string.Empty;



        return text.Substring(start, end - start + 1).Trim();

    }



    private static string CollapseWhitespace(string text) =>

        string.IsNullOrWhiteSpace(text)

            ? string.Empty

            : string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

}

" src\Services\EvaluationService\EvaluationService.Application\Queries\GetPracticeHistory\GetPracticeHistoryHandler.cs:"using EvaluationService.Domain.Repositories;

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

                    AttemptNo = r.AttemptNo,

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

" src\Services\EvaluationService\EvaluationService.Application\Queries\GetPracticeHistory\GetPracticeHistoryQuery.cs:"using MediatR;



namespace EvaluationService.Application.Queries.GetPracticeHistory;



public class GetPracticeHistoryQuery : IRequest<PracticeHistoryResponse>

{

    public string LearnerId { get; set; } = default!;

    public string PatientId { get; set; } = default!;

}

" src\Services\EvaluationService\EvaluationService.Application\Queries\GetPracticeHistory\PracticeHistoryResponse.cs:"namespace EvaluationService.Application.Queries.GetPracticeHistory;



public class PracticeHistoryResponse

{

    public string LearnerId { get; set; } = default!;

    public string PatientId { get; set; } = default!;

    public List<PracticeHistoryItemDto> Items { get; set; } = new();

}



public class PracticeHistoryItemDto

{

    public string PracticeSessionId { get; set; } = default!;

    public int AttemptNo { get; set; }

    public string? EvaluationId { get; set; }

    public decimal? Score { get; set; }

    public int? PureEpaScore { get; set; }

    public int? EntrustmentLevel { get; set; }

    public string? FinalDiagnosis { get; set; }

    public int? Duration { get; set; }

    public string? DiagnosisMatch { get; set; }

    public string? RubricVersion { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = default!;

    public string? FeedbackId { get; set; }

}

" src\Services\EvaluationService\EvaluationService.Application\Services\EpaScoreAggregator.cs:"using EvaluationService.Domain.Entities;

using EvaluationService.Domain.Repositories;

using EvaluationService.Domain.Services;

using EvaluationService.Domain.ValueObjects;

using Microsoft.Extensions.Logging;



namespace EvaluationService.Application.Services;



public sealed class EpaScoreAggregator : IEpaScoreAggregator

{

    private readonly ILogger<EpaScoreAggregator> _logger;



    public EpaScoreAggregator(ILogger<EpaScoreAggregator> logger) => _logger = logger;



    public AggregatedEvaluationResult Aggregate(

        GeminiEvaluationOutput aiOutput,

        EvaluationInput input

    )

    {

        var epaScores = aiOutput

            .EpaScores.Take(5)

            .Select(s => new EvaluationEpaScore

            {

                Id = Guid.NewGuid().ToString("N"),

                EpaId = s.EpaId,

                NumericalScore = Math.Clamp(s.NumericalScore, 0, 20),

                EntrustmentLevel = Math.Clamp(s.EntrustmentLevel, 1, 5),

                FeedbackDetail = s.FeedbackDetail,

                EvidenceCited = s.EvidenceCited ?? [],

                FailurePatterns = s.FailurePatterns ?? [],

                SafetyFlags = s.SafetyFlags ?? [],

                CreatedAt = DateTime.UtcNow,

            })

            .ToList();



        var pureEpaScore = epaScores.Sum(x => x.NumericalScore);



        var allottedTotal = input.AllottedVpTimeMinutes + input.AllottedArgumentTimeMinutes;



        var adjustments = AdjustmentRuleEngine.Calculate(

            diagnosisMatchType: aiOutput.DiagnosisMatchType,

            learnerDiagnosis: input.LearnerFinalDiagnosis,

            canonicalDiagnosis: input.CanonicalDiagnosis,

            actualDurationMinutes: input.ActualDurationMinutes,

            allottedTotalMinutes: allottedTotal,

            warningLabels: input.ActiveWarningLabels,

            warningDescriptions: input.ActiveWarningDescriptions,

            pureEpaScore: pureEpaScore,

            explanation: aiOutput.AdjustmentExplanations

        );



        var finalScore = AdjustmentRuleEngine.ComputeFinalScore(pureEpaScore, adjustments);

        var level = AdjustmentRuleEngine.MapEntrustmentLevel(finalScore);



        var diff = Math.Abs(aiOutput.FinalScore - finalScore);

        if (diff > 5)

            _logger.LogWarning(

                "Score discrepancy: AI={AiScore} backend={BackendScore} diff={Diff} "

                    + "pureEpa={PureEpa} adjTotal={AdjTotal}",

                aiOutput.FinalScore,

                finalScore,

                diff,

                pureEpaScore,

                adjustments.AdjustmentTotal

            );



        return new AggregatedEvaluationResult(

            EpaScores: epaScores,

            PureEpaScore: pureEpaScore,

            PositiveAdjustmentTotal: adjustments.PositiveTotal,

            NegativeAdjustmentTotal: adjustments.NegativeTotal,

            AdjustmentTotal: adjustments.AdjustmentTotal,

            DiagnosisMatchType: aiOutput.DiagnosisMatchType,

            FinalScore: finalScore,

            OverallEntrustmentLevel: level,

            CognitiveAlerts: aiOutput.CognitiveAlerts,

            SafetyEscalationRequired: adjustments.Validation.SafetyEscalationRequired,

            EvaluationTrace: aiOutput.EvaluationTrace,

            Adjustments: adjustments

        );

    }

}

" src\Services\EvaluationService\EvaluationService.Application\Services\FeedbackComposer.cs:"using System.Text;

using System.Text.Json;

using System.Text.Json.Serialization;

using EvaluationService.Application.Dtos;

using EvaluationService.Domain.Entities;

using EvaluationService.Domain.Repositories;

using EvaluationService.Domain.Services;

using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.Logging;



namespace EvaluationService.Application.Services;



public sealed class FeedbackComposer : IFeedbackComposer

{

    private readonly IConfiguration _config;

    private readonly HttpClient _httpClient;

    private readonly IEvaluationRepository _repo;

    private readonly IFeedbackPromptBuilder _feedbackPromptBuilder;

    private readonly ILogger<FeedbackComposer> _logger;



    private const string ModelEndpoint =

        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";



    public FeedbackComposer(

        IConfiguration config,

        IHttpClientFactory httpClientFactory,

        IEvaluationRepository repo,

        IFeedbackPromptBuilder feedbackPromptBuilder,

        ILogger<FeedbackComposer> logger

    )

    {

        _config = config;

        _httpClient = httpClientFactory.CreateClient();

        _repo = repo;

        _feedbackPromptBuilder = feedbackPromptBuilder;

        _logger = logger;

    }



    public async Task<PracticeFeedbackResponseDto> ComposeAsync(

        PracticeSession session,

        Evaluation evaluation,

        List<EvaluationEpaScore> epaScores,

        List<Warning> warnings,

        CancellationToken ct = default

    )

    {

        var clinicalDx = await _repo.GetClinicalDiagnosisByPatientIdAsync(session.PatientId);

        var patient = await _repo.GetVirtualPatientByIdAsync(session.PatientId);



        var prompt = _feedbackPromptBuilder.Build(

            session,

            evaluation,

            epaScores,

            warnings,

            clinicalDx?.CanonicalDiagnosis ?? string.Empty,

            clinicalDx?.DescriptionText ?? string.Empty,

            patient?.TimeSettingMinutes ?? 30,

            patient?.ArgumentTimeMinutes ?? 15

        );

        var apiKey = _config["GeminiAi:ApiKey"] ?? _config["GEMINI_API_KEY"];



        if (string.IsNullOrWhiteSpace(apiKey))

        {

            _logger.LogWarning("Gemini API key missing — returning fallback feedback.");

            return BuildFallbackFeedback(evaluation);

        }



        return await CallGeminiAsync(prompt, apiKey, evaluation, ct);

    }



    private async Task<PracticeFeedbackResponseDto> CallGeminiAsync(

        string prompt,

        string apiKey,

        Evaluation evaluation,

        CancellationToken ct

    )

    {

        var url = $"{ModelEndpoint}?key={apiKey}";

        var body = new

        {

            contents = new[] { new { parts = new[] { new { text = prompt } } } },

            generationConfig = new { temperature = 0.2, responseMimeType = "application/json" },

        };



        try

        {

            var content = new StringContent(

                JsonSerializer.Serialize(body),

                Encoding.UTF8,

                "application/json"

            );

            var response = await _httpClient.PostAsync(url, content, ct);

            response.EnsureSuccessStatusCode();



            var raw = await response.Content.ReadAsStringAsync(ct);

            using var doc = JsonDocument.Parse(raw);

            var text = doc

                .RootElement.GetProperty("candidates")[0]

                .GetProperty("content")

                .GetProperty("parts")[0]

                .GetProperty("text")

                .GetString();



            if (string.IsNullOrWhiteSpace(text))

                return BuildFallbackFeedback(evaluation);



            return ParseFeedback(text);

        }

        catch (Exception ex)

        {

            _logger.LogError(ex, "Gemini feedback generation failed.");

            return BuildFallbackFeedback(evaluation);

        }

    }



    private static PracticeFeedbackResponseDto ParseFeedback(string raw)

    {

        var clean = raw.Trim();



        if (clean.StartsWith("```"))

        {

            var firstNewLine = clean.IndexOf('\n');

            var lastBackticks = clean.LastIndexOf("```");

            if (firstNewLine >= 0 && lastBackticks > firstNewLine)

            {

                clean = clean.Substring(firstNewLine + 1, lastBackticks - firstNewLine - 1).Trim();

            }

        }



        var start = clean.IndexOf('{');

        var end = clean.LastIndexOf('}');

        if (start >= 0 && end > start)

        {

            clean = clean.Substring(start, end - start + 1);

        }



        using var payload = JsonDocument.Parse(clean);

        var root = payload.RootElement;

        var strength = ReadFlexibleText(root, "strength");

        var weakness = ReadFlexibleText(root, "weakness");

        var overallAttemptFeedback = ReadFlexibleText(root, "overallAttemptFeedback");

        var overallLabel = ReadFlexibleText(root, "overallLabel");



        return new PracticeFeedbackResponseDto

        {

            Strength = strength,

            Improvement = weakness,

            OverallAttempt = overallAttemptFeedback,

            OverallLabel = string.IsNullOrWhiteSpace(overallLabel) ? "DEVELOPING" : overallLabel,

        };

    }



    private static string? ReadFlexibleText(JsonElement root, string propertyName)

    {

        if (!root.TryGetProperty(propertyName, out var element))

            return null;



        return element.ValueKind switch

        {

            JsonValueKind.String => element.GetString()?.Trim(),

            JsonValueKind.Array => string.Join(

                "\n",

                element

                    .EnumerateArray()

                    .Select(ToText)

                    .Where(value => !string.IsNullOrWhiteSpace(value))

            ),

            JsonValueKind.Object => ToText(element),

            JsonValueKind.Number => element.ToString(),

            JsonValueKind.True => "true",

            JsonValueKind.False => "false",

            _ => null,

        };

    }



    private static string ToText(JsonElement element)

    {

        return element.ValueKind switch

        {

            JsonValueKind.String => element.GetString() ?? string.Empty,

            JsonValueKind.Array => string.Join(

                " ",

                element

                    .EnumerateArray()

                    .Select(ToText)

                    .Where(value => !string.IsNullOrWhiteSpace(value))

            ),

            JsonValueKind.Object => string.Join(

                " ",

                element

                    .EnumerateObject()

                    .Select(p => ToText(p.Value))

                    .Where(value => !string.IsNullOrWhiteSpace(value))

            ),

            JsonValueKind.Number => element.ToString(),

            JsonValueKind.True => "true",

            JsonValueKind.False => "false",

            _ => string.Empty,

        };

    }



    private static PracticeFeedbackResponseDto BuildFallbackFeedback(Evaluation eval)

    {

        var score = (int)(eval.Score ?? 0);

        return new PracticeFeedbackResponseDto

        {

            Strength = "Feedback requires AI service. Check Gemini API configuration.",

            Improvement = "Review your EPA performance breakdown above for specific areas.",

            OverallAttempt =

                $"Session completed with score {score}/110. Detailed coaching unavailable.",

            OverallLabel = score switch

            {

                >= 90 => "EXCELLENT",

                >= 75 => "GOOD",

                >= 60 => "DEVELOPING",

                _ => "NEEDS_IMPROVEMENT",

            },

        };

    }



    private static string Truncate(string? text, int max) =>

        string.IsNullOrWhiteSpace(text) ? "(empty)"

        : text.Length <= max ? text

        : text.Substring(0, Math.Min(text.Length, max)) + "...[truncated]";

}

" F:\LATEE_BE\lateeBE_visible\src\Services\EvaluationService\EvaluationService.Domain\Entities\Evaluation.cs:"namespace EvaluationService.Domain.Entities;



public class Evaluation

{

    public string Id { get; set; } = default!;

    public string EpaId { get; set; } = default!;

    public string PracticeSessionId { get; set; } = default!;

    public decimal? Score { get; set; }

    public int? Duration { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? FeedbackDetail { get; set; }

    public int? EntrustmentLevel { get; set; }

    public string? RubricVersion { get; set; }

    public int PureEpaScore { get; set; }



    public ICollection<EvaluationEpaScore> EpaScores { get; set; } = [];

}

" src\Services\EvaluationService\EvaluationService.Domain\Entities\EvaluationEpaScore.cs:"namespace EvaluationService.Domain.Entities;



public class EvaluationEpaScore

{

    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string EvaluationId { get; set; } = default!;

    public string EpaId { get; set; } = default!;

    public int NumericalScore { get; set; }

    public int EntrustmentLevel { get; set; }

    public string? FeedbackDetail { get; set; }

    public List<string> EvidenceCited { get; set; } = [];

    public List<string> FailurePatterns { get; set; } = [];

    public List<string> SafetyFlags { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



    public Evaluation? Evaluation { get; set; }

}

" src\Services\EvaluationService\EvaluationService.Domain\Entities\Issue.cs:"namespace EvaluationService.Domain.Entities;



public class Issue

{

    public string Id { get; set; } = Guid.NewGuid().ToString();



    public string PracticeSessionId { get; set; } = string.Empty;



    public string LearnerId { get; set; } = string.Empty;



    public string Label { get; set; } = string.Empty;



    public string Description { get; set; } = string.Empty;



    public string ItemType { get; set; } = string.Empty;



    public DateTime? EditDeadline { get; set; }



    public string Status { get; set; } = "Open";



    public bool IsDeleted { get; set; }



    public DateTime CreatedAt { get; set; }



    public DateTime UpdatedAt { get; set; }

}

" src\Services\EvaluationService\EvaluationService.Domain\Repositories\IAiEvaluationProvider.cs:"using EvaluationService.Domain.Entities;

using EvaluationService.Domain.ValueObjects;



namespace EvaluationService.Domain.Repositories;



public interface IAiEvaluationProvider

{

    Task<GeminiEvaluationOutput> AnalyzePerformanceAsync(

        string prompt,

        CancellationToken ct = default

    );

}



public sealed record EvaluationInput(

    string SessionId,

    string LearnerId,

    string PatientId,

    string VpConversationLog,

    string AiReasoningLog,

    string LearnerFinalDiagnosis,

    string CanonicalDiagnosis,

    string CaseDescription,

    int AllottedVpTimeMinutes,

    int AllottedArgumentTimeMinutes,

    int ActualDurationMinutes,

    string RubricContent,

    string RubricVersion,

    List<string> ActiveWarningLabels,

    List<string> ActiveWarningDescriptions

);



public sealed record GeminiEvaluationOutput(

    List<EvaluationEpaScore> EpaScores,

    int DiagnosisModifier,

    string DiagnosisMatchType,

    int TimeModifier,

    int TotalWarningPenalty,

    int FinalScore,

    int OverallEntrustmentLevel,

    List<string> CognitiveAlerts,

    bool SafetyEscalationRequired,

    string EvaluationTrace,

    AdjustmentExplanation? AdjustmentExplanations

);



public sealed record AdjustmentExplanation(

    string? Diagnosis,

    string? Time,

    List<WarningExplanation> Warnings

);



public sealed record WarningExplanation(string Label, string Reason);

" src\Services\EvaluationService\EvaluationService.Domain\Repositories\IEvaluationRepository.cs:"using EvaluationService.Domain.Entities;



namespace EvaluationService.Domain.Repositories;



public interface IEvaluationRepository

{

    Task<Evaluation?> GetByIdAsync(string id);

    Task<List<Evaluation>> GetByLearnerIdAsync(string learnerId);



    Task<PracticeSession?> GetPracticeSessionByIdAsync(string id);

    Task<List<Warning>> GetWarningsByPracticeSessionIdAsync(string practiceSessionId);

    Task<ClinicalCaseDiagnosisDto?> GetClinicalDiagnosisByPatientIdAsync(string patientId);

    Task<VirtualPatientRef?> GetVirtualPatientByIdAsync(string patientId);

    Task<RubricDto?> GetRubricByEccIdAsync(string eccId);

    Task<PracticeFeedback?> GetPracticeFeedbackBySessionIdAsync(string practiceSessionId);



    Task<List<EvaluationEpaScore>> GetEpaScoresByEvaluationIdAsync(string evaluationId);



    Task<List<PracticeHistoryRow>> GetPracticeHistoryAsync(

        string learnerId,

        string patientId,

        CancellationToken cancellationToken = default

    );



    Task<List<IssueListItem>> GetIssuesAsync(string practiceSessionId, string learnerId);

    Task AddIssueAsync(Issue issue);



    Task AddEvaluationAsync(Evaluation evaluation);

    Task AddEpaScoresAsync(IEnumerable<EvaluationEpaScore> scores);

    Task AddWarningsAsync(IEnumerable<Warning> warnings);

    Task AddPracticeFeedbackAsync(PracticeFeedback feedback);

    Task UpdatePracticeSessionAsync(PracticeSession session);

    Task DeleteAsync(string id);

    Task SaveChangesAsync();

}



public record ClinicalCaseDiagnosisDto(

    string CaseId,

    string EccId,

    string CanonicalDiagnosis,

    string DescriptionText,

    string Symptom,

    string MedicalHistory

);



public record RubricDto(string Id, string Description, string Version);



public record IssueListItem(

    string IssueId,

    string LearnerId,

    string LearnerName,

    DateTime CreatedAt,

    string? Label,

    string Description,

    string Status,

    IssueExpertFeedback? ExpertFeedback

);



public record IssueExpertFeedback(string ExpertId, string ExpertName, string Feedback);



public record PracticeHistoryRow(

    string PracticeSessionId,

    int AttemptNo,

    string? EvaluationId,

    decimal? Score,

    int? PureEpaScore,

    int? EntrustmentLevel,

    string? FinalDiagnosis,

    int? Duration,

    string? DiagnosisMatch,

    string? RubricVersion,

    DateTime CreatedAt,

    string Status,

    string? FeedbackId

);

" src\Services\EvaluationService\EvaluationService.Domain\Services\IEpaScoreAggregator.cs:"using EvaluationService.Domain.Entities;

using EvaluationService.Domain.Repositories;

using EvaluationService.Domain.ValueObjects;



namespace EvaluationService.Domain.Services;



public interface IEpaScoreAggregator

{

    AggregatedEvaluationResult Aggregate(GeminiEvaluationOutput aiOutput, EvaluationInput input);

}



/// finalScore = CLAMP(pureEpaScore + adjustments.AdjustmentTotal, 0, 110)

public sealed record AggregatedEvaluationResult(

    List<EvaluationEpaScore> EpaScores,

    int PureEpaScore,

    int PositiveAdjustmentTotal,

    int NegativeAdjustmentTotal,

    int AdjustmentTotal,

    string DiagnosisMatchType,

    int FinalScore,

    int OverallEntrustmentLevel,

    List<string> CognitiveAlerts,

    bool SafetyEscalationRequired,

    string EvaluationTrace,

    ScoringAdjustments Adjustments

);

" src\Services\EvaluationService\EvaluationService.Domain\Services\IEvaluationPromptBuilder.cs:"using EvaluationService.Domain.Repositories;

using EvaluationService.Domain.ValueObjects;



namespace EvaluationService.Domain.Services;



/// Prompt AI evaluator từ input + rubric context.

public interface IEvaluationPromptBuilder

{

    string Build(EvaluationInput input, RubricContext rubric);

}

" src\Services\EvaluationService\EvaluationService.Domain\ValueObjects\RubricContext.cs:"namespace EvaluationService.Domain.ValueObjects;



public sealed record RubricContext(

    string EccId,

    string Version,

    string FullContent,

    bool IsAvailable

)

{

    public static RubricContext Empty(string eccId) =>

        new(EccId: eccId, Version: "unknown", FullContent: string.Empty, IsAvailable: false);

}

" src\Services\EvaluationService\EvaluationService.Domain\ValueObjects\AdjustmentRuleEngine.cs:"using EvaluationService.Domain.Repositories;



namespace EvaluationService.Domain.ValueObjects;



/// Tách hoàn toàn khỏi EPA scoring.

/// finalScore = CLAMP(pureEpaScore + adjustments.AdjustmentTotal, 0, 110)

///

/// WARNING LABELS (từ FE warnings[])

///   RED_FLAG_MISSED, DANGEROUS_MISDIAGNOSIS, PREMATURE_CLOSURE,

///   PATIENT_SAFETY_BREACH, OVERCONFIDENCE, ANCHORING_BIAS, COMMUNICATION_VIOLATION

///

/// VALIDATION CATEGORIES (AI classify từng turn trong transcript)

///   failurePatterns[] của EpaScore, KHÔNG đi qua engine này:

///   valid, ethics_violation, workflow_violation, unsafe_question,

///   irrelevant_question, clinical_reasoning_issue

public static class AdjustmentRuleEngine

{

    public static ScoringAdjustments Calculate(

        string diagnosisMatchType,

        string learnerDiagnosis,

        string canonicalDiagnosis,

        int actualDurationMinutes,

        int allottedTotalMinutes,

        IReadOnlyList<string> warningLabels,

        IReadOnlyList<string> warningDescriptions,

        int pureEpaScore,

        AdjustmentExplanation? explanation = null

    )

    {

        var positive = new List<ScoringAdjustment>();

        var negative = new List<ScoringAdjustment>();



        var warningReasons =

            explanation

                ?.Warnings?.Where(w => !string.IsNullOrWhiteSpace(w.Label))

                .GroupBy(w => w.Label.Trim().ToUpperInvariant())

                .ToDictionary(g => g.Key, g => g.Last().Reason ?? string.Empty)

            ?? new Dictionary<string, string>();



        // ── 1. Diagnosis modifier ────────────────────────────────────────

        ApplyDiagnosisAdjustment(

            diagnosisMatchType,

            learnerDiagnosis,

            canonicalDiagnosis,

            explanation?.Diagnosis,

            positive,

            negative

        );



        // ── 2. Time modifier ─────────────────────────────────────────────

        // ApplyTimeAdjustment(

        //     actualDurationMinutes,

        //     allottedTotalMinutes,

        //     pureEpaScore,

        //     explanation?.Time,

        //     positive,

        //     negative

        // );



        // ── 3. Warning penalties ─────────────────────────────────────────

        var (safetyEscalation, warningPenaltyTotal) = ApplyWarningAdjustments(

            warningLabels,

            warningDescriptions,

            warningReasons,

            negative

        );



        // Cap tổng warning penalty tại 25

        CapWarningPenalty(negative, warningPenaltyTotal);



        // ── 4. Validation summary ────────────────────────────────────────

        var validation = new ValidationSummary(

            HasEthicsViolation: warningLabels.Any(l =>

                l.Contains("ETHICS", StringComparison.OrdinalIgnoreCase)

            ),

            HasUnsafeQuestion: warningLabels.Any(l =>

                l.Contains("UNSAFE", StringComparison.OrdinalIgnoreCase)

                || l.Equals("PATIENT_SAFETY_BREACH", StringComparison.OrdinalIgnoreCase)

            ),

            HasWorkflowViolation: warningLabels.Any(l =>

                l.Contains("WORKFLOW", StringComparison.OrdinalIgnoreCase)

                || l.Equals("COMMUNICATION_VIOLATION", StringComparison.OrdinalIgnoreCase)

            ),

            SafetyEscalationRequired: safetyEscalation,

            TotalWarnings: warningLabels.Count

        );



        return new ScoringAdjustments(positive.AsReadOnly(), negative.AsReadOnly(), validation);

    }



    private static void ApplyDiagnosisAdjustment(

        string matchType,

        string learnerDx,

        string canonicalDx,

        string? aiReason,

        List<ScoringAdjustment> positive,

        List<ScoringAdjustment> negative

    )

    {

        var normalized = matchType.Trim().ToUpperInvariant();



        string DynamicReason(string template) =>

            string.IsNullOrWhiteSpace(learnerDx)

                ? template

                : $"{template} Learner submitted: \"{learnerDx}\". Canonical: \"{canonicalDx}\".";



        string Reason(string template) =>

            string.IsNullOrWhiteSpace(aiReason) ? DynamicReason(template) : aiReason;



        switch (normalized)

        {

            case "EXACT_MATCH":

                positive.Add(

                    new(

                        Code: "DIAGNOSIS_EXACT_MATCH",

                        Title: "Exact diagnosis match",

                        Score: +10,

                        Reason: Reason(

                            "Learner's diagnosis exactly matches the canonical diagnosis."

                        ),

                        Source: "diagnosis",

                        Severity: "positive"

                    )

                );

                break;



            case "SEMANTIC_MATCH":

                positive.Add(

                    new(

                        Code: "DIAGNOSIS_SEMANTIC_MATCH",

                        Title: "Semantic diagnosis match",

                        Score: +10,

                        Reason: Reason(

                            "Learner's diagnosis is clinically equivalent to the canonical diagnosis."

                        ),

                        Source: "diagnosis",

                        Severity: "positive"

                    )

                );

                break;



            case "PARTIAL_MATCH":

                positive.Add(

                    new(

                        Code: "DIAGNOSIS_PARTIAL_MATCH",

                        Title: "Partial diagnosis match",

                        Score: +5,

                        Reason: Reason(

                            "Learner identified the correct organ system or disease category but missed specifics."

                        ),

                        Source: "diagnosis",

                        Severity: "positive"

                    )

                );

                break;



            case "WRONG":

                negative.Add(

                    new(

                        Code: "DIAGNOSIS_WRONG",

                        Title: "Incorrect diagnosis",

                        Score: -10,

                        Reason: Reason(

                            "Learner's diagnosis does not match the canonical diagnosis and reflects a clinical reasoning error."

                        ),

                        Source: "diagnosis",

                        Severity: "high"

                    )

                );

                break;



            case "DANGEROUS":

                negative.Add(

                    new(

                        Code: "DIAGNOSIS_DANGEROUS",

                        Title: "Dangerous misdiagnosis",

                        Score: -20,

                        Reason: Reason(

                            "Learner's diagnosis is clinically dangerous and could cause patient harm if acted upon."

                        ),

                        Source: "diagnosis",

                        Severity: "critical"

                    )

                );

                break;



            case "NO_DIAGNOSIS":

                negative.Add(

                    new(

                        Code: "DIAGNOSIS_MISSING",

                        Title: "No diagnosis submitted",

                        Score: -15,

                        Reason: Reason(

                            "Learner did not submit a final diagnosis before ending the session."

                        ),

                        Source: "diagnosis",

                        Severity: "high"

                    )

                );

                break;



            // UNKNOWN / UNVERIFIED → no adjustment, no entry

        }

    }



    // private static void ApplyTimeAdjustment(

    //     int actualMinutes,

    //     int allottedMinutes,

    //     int pureEpaScore,

    //     string? aiReason,

    //     List<ScoringAdjustment> positive,

    //     List<ScoringAdjustment> negative

    // )

    // {

    //     if (allottedMinutes <= 0)

    //         return;



    //     var ratio = (double)actualMinutes / allottedMinutes;



    //     // string TimeReason(string context) =>

    //     //     string.IsNullOrWhiteSpace(aiReason)

    //     //         ? $"{context} Session used {actualMinutes} min out of {allottedMinutes} min allotted (ratio: {ratio:F2})."

    //     //         : aiReason;



    //     // if (ratio < 0.40)

    //     // {

    //     //     negative.Add(

    //     //         new(

    //     //             Code: "TIME_TOO_SHORT",

    //     //             Title: "Session suspiciously short",

    //     //             Score: -3,

    //     //             Reason: TimeReason(

    //     //                 "Session completed in less than 40% of allotted time, suggesting incomplete evaluation."

    //     //             ),

    //     //             Source: "time",

    //     //             Severity: "medium"

    //     //         )

    //     //     );

    //     // }

    //     // else if (ratio < 0.60)

    //     // {

    //     //     if (pureEpaScore >= 60)

    //     //         positive.Add(

    //     //             new(

    //     //                 Code: "TIME_EFFICIENT",

    //     //                 Title: "High time efficiency",

    //     //                 Score: +3,

    //     //                 Reason: TimeReason(

    //     //                     "Learner completed the session efficiently with high clinical performance."

    //     //                 ),

    //     //                 Source: "time",

    //     //                 Severity: "positive"

    //     //             )

    //     //         );

    //     // }

    //     // else if (ratio < 0.80)

    //     // {

    //     //     positive.Add(

    //     //         new(

    //     //             Code: "TIME_GOOD",

    //     //             Title: "Good time management",

    //     //             Score: +2,

    //     //             Reason: TimeReason(

    //     //                 "Learner completed the session within a well-managed time frame."

    //     //             ),

    //     //             Source: "time",

    //     //             Severity: "positive"

    //     //         )

    //     //     );

    //     // }

    //     // else if (ratio <= 1.00)

    //     // {

    //     //     // On time

    //     // }

    //     // else if (ratio <= 1.20)

    //     // {

    //     //     negative.Add(

    //     //         new(

    //     //             Code: "TIME_OVER_SLIGHT",

    //     //             Title: "Slightly over time",

    //     //             Score: -1,

    //     //             Reason: TimeReason("Session exceeded allotted time by up to 20%."),

    //     //             Source: "time",

    //     //             Severity: "low"

    //     //         )

    //     //     );

    //     // }

    //     // else

    //     // {

    //     //     negative.Add(

    //     //         new(

    //     //             Code: "TIME_OVER_SIGNIFICANT",

    //     //             Title: "Significantly over time",

    //     //             Score: -3,

    //     //             Reason: TimeReason(

    //     //                 "Session exceeded allotted time by more than 20%, indicating poor time management."

    //     //             ),

    //     //             Source: "time",

    //     //             Severity: "medium"

    //     //         )

    //     //     );

    //     // }

    // }



    private static (bool safetyEscalation, int totalPenalty) ApplyWarningAdjustments(

        IReadOnlyList<string> labels,

        IReadOnlyList<string> descriptions,

        IReadOnlyDictionary<string, string> aiReasons,

        List<ScoringAdjustment> negative

    )

    {

        bool safetyEscalation = false;

        int totalPenalty = 0;



        for (int i = 0; i < labels.Count; i++)

        {

            var label = labels[i];

            var description = i < descriptions.Count ? descriptions[i] : string.Empty;



            var normalized = label.ToUpperInvariant();

            var (penalty, isSafety, title, baseReason) = normalized switch

            {

                "RED_FLAG_MISSED" => (

                    3,

                    false,

                    "Red flag missed",

                    "Learner failed to identify or ask about a clinically significant red flag symptom."

                ),



                "DANGEROUS_MISDIAGNOSIS" => (

                    10,

                    true,

                    "Dangerous misdiagnosis warning",

                    "A potentially dangerous diagnostic error was flagged during the session."

                ),



                "PREMATURE_CLOSURE" => (

                    4,

                    false,

                    "Premature clinical closure",

                    "Learner stopped diagnostic reasoning before adequately exploring differential diagnoses."

                ),



                "PATIENT_SAFETY_BREACH" => (

                    8,

                    true,

                    "Patient safety breach",

                    "An action or recommendation that could directly compromise patient safety was detected."

                ),



                "OVERCONFIDENCE" => (

                    2,

                    false,

                    "Overconfidence bias",

                    "Learner expressed certainty disproportionate to the evidence available in the case."

                ),



                "ANCHORING_BIAS" => (

                    3,

                    false,

                    "Anchoring bias",

                    "Learner fixated on an initial hypothesis and failed to adequately reconsider when contradicting evidence emerged."

                ),



                "COMMUNICATION_VIOLATION" => (

                    2,

                    false,

                    "Communication violation",

                    "Learner used inappropriate, unsafe, or unprofessional language during the patient interaction."

                ),



                _ => (0, false, string.Empty, string.Empty),

            };



            if (penalty == 0)

                continue;



            if (isSafety)

                safetyEscalation = true;

            totalPenalty += penalty;



            var fullReason =

                aiReasons.TryGetValue(normalized, out var aiReason)

                && !string.IsNullOrWhiteSpace(aiReason)

                    ? aiReason

                : string.IsNullOrWhiteSpace(description) ? baseReason

                : $"{baseReason} Details: {description}";



            negative.Add(

                new(

                    Code: label.ToUpperInvariant(),

                    Title: title,

                    Score: -penalty,

                    Reason: fullReason,

                    Source: "warning",

                    Severity: isSafety ? "critical"

                        : penalty >= 4 ? "high"

                        : "medium"

                )

            );

        }



        return (safetyEscalation, totalPenalty);

    }



    private static void CapWarningPenalty(List<ScoringAdjustment> negative, int totalPenalty)

    {

        if (totalPenalty <= 25)

            return;



        var excess = totalPenalty - 25;

        var lastWarn = negative.FindLastIndex(a => a.Source == "warning");

        if (lastWarn < 0)

            return;



        var last = negative[lastWarn];

        negative[lastWarn] = last with

        {

            Score = last.Score + excess,

            Reason =

                last.Reason + $" (Warning penalty capped at 25 total; {excess} points reduced.)",

        };

    }



    // ────────────────────────────────────────────────────────────────────

    public static int ComputeFinalScore(int pureEpaScore, ScoringAdjustments adj) =>

        Math.Clamp(pureEpaScore + adj.AdjustmentTotal, 0, 110);



    public static int MapEntrustmentLevel(int finalScore) =>

        finalScore switch

        {

            <= 39 => 1,

            <= 59 => 2,

            <= 74 => 3,

            <= 89 => 4,

            _ => 5,

        };

}

" src\Services\EvaluationService\EvaluationService.Infrastructure\Persistence\EvaluationDbContext.cs:"using System.Text.Json;

using EvaluationService.Domain.Entities;

using Microsoft.EntityFrameworkCore;



namespace EvaluationService.Infrastructure.Persistence;



public class EvaluationDbContext : DbContext

{

    public EvaluationDbContext(DbContextOptions<EvaluationDbContext> options)

        : base(options) { }



    public DbSet<Evaluation> Evaluations => Set<Evaluation>();

    public DbSet<EvaluationEpaScore> EpaScores => Set<EvaluationEpaScore>();

    public DbSet<Warning> Warnings => Set<Warning>();

    public DbSet<PracticeSession> PracticeSessions => Set<PracticeSession>();

    public DbSet<PracticeFeedback> PracticeFeedbacks => Set<PracticeFeedback>();

    public DbSet<Issue> Issues => Set<Issue>();

    public DbSet<ResolvedIssue> ResolvedIssues => Set<ResolvedIssue>();



    protected override void OnModelCreating(ModelBuilder b)

    {

        base.OnModelCreating(b);



        b.Entity<Evaluation>(e =>

        {

            e.ToTable("evaluation");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id");

            e.Property(x => x.EpaId).HasColumnName("epa_id");

            e.Property(x => x.PracticeSessionId).HasColumnName("practice_session_id");

            e.Property(x => x.Score).HasColumnName("score").HasPrecision(5, 2);

            e.Property(x => x.Duration).HasColumnName("duration");

            e.Property(x => x.CreatedAt).HasColumnName("created_at");

            e.Property(x => x.FeedbackDetail).HasColumnName("feedback_detail");

            e.Property(x => x.EntrustmentLevel).HasColumnName("entrustment_level");

            e.Property(x => x.RubricVersion).HasColumnName("rubric_version").HasMaxLength(20);

            e.Property(x => x.PureEpaScore).HasColumnName("pure_epa_score").HasDefaultValue(0);

            e.HasMany(x => x.EpaScores)

                .WithOne(x => x.Evaluation)

                .HasForeignKey(x => x.EvaluationId)

                .OnDelete(DeleteBehavior.Cascade);

        });



        b.Entity<EvaluationEpaScore>(e =>

        {

            e.ToTable("evaluation_epa_score");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id").HasMaxLength(50);

            e.Property(x => x.EvaluationId)

                .HasColumnName("evaluation_id")

                .HasMaxLength(50)

                .IsRequired();

            e.Property(x => x.EpaId).HasColumnName("epa_id").HasMaxLength(20).IsRequired();

            e.Property(x => x.NumericalScore).HasColumnName("numerical_score");

            e.Property(x => x.EntrustmentLevel).HasColumnName("entrustment_level");

            e.Property(x => x.FeedbackDetail).HasColumnName("feedback_detail");

            e.Property(x => x.CreatedAt).HasColumnName("created_at");

            e.Property(x => x.EvidenceCited)

                .HasColumnName("evidence_cited")

                .HasColumnType("json")

                .HasConversion(

                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),

                    v =>

                        JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)

                        ?? new List<string>()

                );

            e.Property(x => x.FailurePatterns)

                .HasColumnName("failure_patterns")

                .HasColumnType("json")

                .HasConversion(

                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),

                    v =>

                        JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)

                        ?? new List<string>()

                );

            e.Property(x => x.SafetyFlags)

                .HasColumnName("safety_flags")

                .HasColumnType("json")

                .HasConversion(

                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),

                    v =>

                        JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)

                        ?? new List<string>()

                );

        });



        b.Entity<Warning>(e =>

        {

            e.ToTable("warning");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id");

            e.Property(x => x.PracticeSessionId).HasColumnName("practice_session_id").IsRequired();

            e.Property(x => x.LearnerId).HasColumnName("learner_id").IsRequired();

            e.Property(x => x.Label).HasColumnName("label");

            e.Property(x => x.Description).HasColumnName("description");

            e.Property(x => x.CreatedAt).HasColumnName("created_at");

        });



        b.Entity<PracticeSession>(e =>

        {

            e.ToTable("practice_sessions");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id)

                .HasColumnName("id")

                .HasMaxLength(50)

                .ValueGeneratedNever()

                .IsRequired();

            e.Property(x => x.LearnerId).HasColumnName("learner_id").HasMaxLength(50).IsRequired();

            e.Property(x => x.PatientId).HasColumnName("patient_id").HasMaxLength(50).IsRequired();

            e.Property(x => x.FinalDiagnosis).HasColumnName("final_diagnosis");

            e.Property(x => x.AiReasoningLog)

                .HasColumnName("ai_reasoning_log")

                .HasColumnType("json");

            e.Property(x => x.VpConversationLog)

                .HasColumnName("vp_conversation_log")

                .HasColumnType("json");

            e.Property(x => x.ModuleId).HasColumnName("module_id");

            e.Property(x => x.DiscussionType).HasColumnName("discussion_type");

            e.Property(x => x.GuidelinesId).HasColumnName("guidelines_id");

            e.Property(x => x.StartTime)

                .HasColumnName("start_time")

                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.Property(x => x.EndTime).HasColumnName("end_time");

            e.Property(x => x.Status).HasColumnName("status").HasDefaultValue("Practicing");

            e.Property(x => x.CreatedAt).HasColumnName("created_at");

        });



        b.Entity<PracticeFeedback>(e =>

        {

            e.ToTable("practice_feedback");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id").HasMaxLength(50);

            e.Property(x => x.OverallAttempt).HasColumnName("overall_attempt");

            e.Property(x => x.OverallLabel).HasColumnName("overall_label");

            e.Property(x => x.Strength).HasColumnName("strength");

            e.Property(x => x.Improvement).HasColumnName("improvement");

            e.Property(x => x.CreatedAt)

                .HasColumnName("created_at")

                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.Property(x => x.UpdatedAt)

                .HasColumnName("updated_at")

                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            e.Property(x => x.EvaluationId)

                .HasColumnName("evaluation_id")

                .HasMaxLength(50)

                .IsRequired();

            e.Property(x => x.PracticeSessionId)

                .HasColumnName("practice_session_id")

                .HasMaxLength(50)

                .IsRequired();

        });



        b.Entity<Issue>(e =>

        {

            e.ToTable("issue");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id").HasMaxLength(50);

            e.Property(x => x.PracticeSessionId).HasColumnName("practice_session_id");

            e.Property(x => x.LearnerId).HasColumnName("learner_id").HasMaxLength(50).IsRequired();

            e.Property(x => x.ItemType).HasColumnName("ItemType").HasMaxLength(20).IsRequired();

            e.Property(x => x.IsDeleted).HasColumnName("is_deleted");

            e.Property(x => x.EditDeadline).HasColumnName("editDeadline");

            e.Property(x => x.Description).HasColumnName("description");

            e.Property(x => x.Label).HasColumnName("label");

            e.Property(x => x.Status).HasColumnName("status");

            e.Property(x => x.CreatedAt).HasColumnName("created_at");

            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        });



        b.Entity<ResolvedIssue>(e =>

        {

            e.ToTable("resolved_issue");

            e.HasKey(x => new { x.IssueId, x.ExpertId });

            e.Property(x => x.IssueId).HasColumnName("issue_id").HasMaxLength(50).IsRequired();

            e.Property(x => x.ExpertId).HasColumnName("expert_id").HasMaxLength(50).IsRequired();

            e.Property(x => x.Feedback).HasColumnName("feedback");

        });

    }

}

" src\Services\EvaluationService\EvaluationService.Infrastructure\Repositories\EvaluationRepository.cs:"using EvaluationService.Domain.Entities;

using EvaluationService.Domain.Repositories;

using EvaluationService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;



namespace EvaluationService.Infrastructure.Repositories;



public class EvaluationRepository : IEvaluationRepository

{

    private readonly EvaluationDbContext _db;



    public EvaluationRepository(EvaluationDbContext db) => _db = db;



    public async Task<Evaluation?> GetByIdAsync(string id) =>

        await _db.Evaluations.Include(e => e.EpaScores).FirstOrDefaultAsync(x => x.Id == id);



    public async Task<List<Evaluation>> GetByLearnerIdAsync(string learnerId) =>

        await _db

            .Evaluations.Join(

                _db.PracticeSessions,

                eval => eval.PracticeSessionId,

                session => session.Id,

                (eval, session) => new { eval, session }

            )

            .Where(x => x.session.LearnerId == learnerId)

            .OrderByDescending(x => x.eval.CreatedAt)

            .Select(x => x.eval)

            .AsNoTracking()

            .ToListAsync();



    public async Task<PracticeSession?> GetPracticeSessionByIdAsync(string id) =>

        await _db.PracticeSessions.FirstOrDefaultAsync(x => x.Id == id);



    public async Task<List<Warning>> GetWarningsByPracticeSessionIdAsync(

        string practiceSessionId

    ) =>

        await _db

            .Warnings.AsNoTracking()

            .Where(x => x.PracticeSessionId == practiceSessionId)

            .OrderByDescending(x => x.CreatedAt)

            .ToListAsync();



    public async Task<ClinicalCaseDiagnosisDto?> GetClinicalDiagnosisByPatientIdAsync(

        string patientId

    )

    {

        var result = await _db

            .Database.SqlQuery<ClinicalCaseDiagnosisRaw>(

                $"""

                SELECT

                    cc.case_id        AS CaseId,

                    cc.eccid          AS EccId,

                    cc.type           AS CanonicalDiagnosis,

                    cc.description    AS DescriptionText,

                    cc.symptom        AS Symptom,

                    cc.medicalhistory AS MedicalHistory

                FROM virtual_patient vp

                INNER JOIN clinical_case cc ON vp.case_id = cc.case_id

                WHERE vp.patient_id = {patientId}

                LIMIT 1

                """

            )

            .FirstOrDefaultAsync();



        if (result == null)

            return null;



        return new ClinicalCaseDiagnosisDto(

            CaseId: result.CaseId ?? string.Empty,

            EccId: result.EccId ?? string.Empty,

            CanonicalDiagnosis: result.CanonicalDiagnosis ?? string.Empty,

            DescriptionText: result.DescriptionText ?? string.Empty,

            Symptom: result.Symptom ?? string.Empty,

            MedicalHistory: result.MedicalHistory ?? string.Empty

        );

    }



    public async Task<VirtualPatientRef?> GetVirtualPatientByIdAsync(string patientId)

    {

        var result = await _db

            .Database.SqlQuery<VirtualPatientRaw>(

                $"""

                SELECT

                    patient_id    AS PatientId,

                    time_setting  AS TimeSettingMinutes,

                    argument_time AS ArgumentTimeMinutes

                FROM virtual_patient

                WHERE patient_id = {patientId}

                LIMIT 1

                """

            )

            .FirstOrDefaultAsync();



        if (result == null)

            return null;



        return new VirtualPatientRef(

            PatientId: result.PatientId ?? string.Empty,

            TimeSettingMinutes: result.TimeSettingMinutes ?? 30,

            ArgumentTimeMinutes: result.ArgumentTimeMinutes ?? 15

        );

    }



    public async Task<RubricDto?> GetRubricByEccIdAsync(string eccId)

    {

        if (string.IsNullOrWhiteSpace(eccId))

            return null;



        var result = await _db

            .Database.SqlQuery<RubricRaw>(

                $"""

                SELECT id, description, version

                FROM evaluation_clinical_criteria

                WHERE id = {eccId}

                LIMIT 1

                """

            )

            .FirstOrDefaultAsync();



        if (result == null)

            return null;



        return new RubricDto(

            Id: result.Id ?? string.Empty,

            Description: result.Description ?? string.Empty,

            Version: result.Version ?? "1.0.0"

        );

    }



    public async Task<PracticeFeedback?> GetPracticeFeedbackBySessionIdAsync(

        string practiceSessionId

    ) =>

        await _db

            .PracticeFeedbacks.AsNoTracking()

            .FirstOrDefaultAsync(x => x.PracticeSessionId == practiceSessionId);



    public async Task<List<EvaluationEpaScore>> GetEpaScoresByEvaluationIdAsync(

        string evaluationId

    ) =>

        await _db

            .EpaScores.AsNoTracking()

            .Where(x => x.EvaluationId == evaluationId)

            .OrderBy(x => x.EpaId)

            .ToListAsync();



    public async Task<List<PracticeHistoryRow>> GetPracticeHistoryAsync(

        string learnerId,

        string patientId,

        CancellationToken cancellationToken = default

    )

    {

        var rows = await _db

            .Database.SqlQuery<PracticeHistoryRaw>(

                $"""

                SELECT

                    ps.id AS PracticeSessionId,

                    ROW_NUMBER() OVER (

                        PARTITION BY ps.learner_id, ps.patient_id

                        ORDER BY ps.created_at ASC, ps.id ASC

                    ) AS AttemptNo,

                    e.id AS EvaluationId,

                    e.score AS Score,

                    e.pure_epa_score AS PureEpaScore,

                    e.entrustment_level AS EntrustmentLevel,

                    ps.final_diagnosis AS FinalDiagnosis,

                    e.duration AS Duration,

                    NULL AS DiagnosisMatch,

                    e.rubric_version AS RubricVersion,

                    ps.created_at AS CreatedAt,

                    ps.status AS Status,

                    pf.id AS FeedbackId

                FROM practice_sessions ps

                LEFT JOIN evaluation e ON e.practice_session_id = ps.id

                LEFT JOIN practice_feedback pf ON pf.practice_session_id = ps.id

                WHERE ps.learner_id = {learnerId}

                    AND ps.patient_id = {patientId}

                ORDER BY ps.created_at ASC

                """

            )

            .ToListAsync(cancellationToken);



        return rows.Select(r => new PracticeHistoryRow(

                PracticeSessionId: r.PracticeSessionId ?? string.Empty,

                AttemptNo: r.AttemptNo ?? 0,

                EvaluationId: r.EvaluationId,

                Score: r.Score,

                PureEpaScore: r.PureEpaScore,

                EntrustmentLevel: r.EntrustmentLevel,

                FinalDiagnosis: r.FinalDiagnosis,

                Duration: r.Duration,

                DiagnosisMatch: r.DiagnosisMatch,

                RubricVersion: r.RubricVersion,

                CreatedAt: r.CreatedAt ?? DateTime.UtcNow,

                Status: r.Status ?? string.Empty,

                FeedbackId: r.FeedbackId

            ))

            .ToList();

    }



    public async Task<List<IssueListItem>> GetIssuesAsync(

        string practiceSessionId,

        string learnerId

    )

    {

        var rows = await _db

            .Database.SqlQuery<IssueRow>(

                $"""

                SELECT

                    i.id AS IssueId,

                    i.learner_id AS LearnerId,

                    u.name AS LearnerName,

                    i.created_at AS CreatedAt,

                    i.label AS Label,

                    i.description AS Description,

                    i.status AS Status,

                    ri.expert_id AS ExpertId,

                    eu.name AS ExpertName,

                    ri.feedback AS Feedback

                FROM issue i

                INNER JOIN users u ON u.userid = i.learner_id

                LEFT JOIN resolved_issue ri ON ri.issue_id = i.id

                LEFT JOIN users eu ON eu.userid = ri.expert_id

                WHERE i.is_deleted = false

                    AND i.practice_session_id = {practiceSessionId}

                    AND i.learner_id = {learnerId}

                ORDER BY i.created_at DESC

                """

            )

            .ToListAsync();



        return rows.GroupBy(r => new

            {

                r.IssueId,

                r.LearnerId,

                r.LearnerName,

                r.CreatedAt,

                r.Label,

                r.Description,

                r.Status,

            })

            .Select(g =>

            {

                var firstExpert = g.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ExpertId));

                IssueExpertFeedback? expertFeedback = null;

                if (firstExpert != null)

                {

                    expertFeedback = new IssueExpertFeedback(

                        ExpertId: firstExpert.ExpertId ?? string.Empty,

                        ExpertName: firstExpert.ExpertName ?? string.Empty,

                        Feedback: firstExpert.Feedback ?? string.Empty

                    );

                }



                return new IssueListItem(

                    IssueId: g.Key.IssueId ?? string.Empty,

                    LearnerId: g.Key.LearnerId ?? string.Empty,

                    LearnerName: g.Key.LearnerName ?? string.Empty,

                    CreatedAt: g.Key.CreatedAt ?? DateTime.UtcNow,

                    Label: g.Key.Label,

                    Description: g.Key.Description ?? string.Empty,

                    Status: g.Key.Status ?? "Open",

                    ExpertFeedback: expertFeedback

                );

            })

            .ToList();

    }



    public async Task AddEvaluationAsync(Evaluation evaluation) =>

        await _db.Evaluations.AddAsync(evaluation);



    public async Task AddEpaScoresAsync(IEnumerable<EvaluationEpaScore> scores) =>

        await _db.EpaScores.AddRangeAsync(scores);



    public async Task AddWarningsAsync(IEnumerable<Warning> warnings)

    {

        var incomingIds = warnings.Select(w => w.Id).ToList();

        var existingIds = await _db

            .Warnings.Where(w => incomingIds.Contains(w.Id))

            .Select(w => w.Id)

            .ToListAsync();



        var existingSet = new HashSet<string>(existingIds);

        var newWarnings = warnings.Where(w => !existingSet.Contains(w.Id)).ToList();



        if (newWarnings.Count > 0)

            await _db.Warnings.AddRangeAsync(newWarnings);

    }



    public async Task AddPracticeFeedbackAsync(PracticeFeedback feedback) =>

        await _db.PracticeFeedbacks.AddAsync(feedback);



    public async Task AddIssueAsync(Issue issue) => await _db.Issues.AddAsync(issue);



    public Task UpdatePracticeSessionAsync(PracticeSession session)

    {

        _db.PracticeSessions.Update(session);

        return Task.CompletedTask;

    }



    public async Task DeleteAsync(string id)

    {

        var entity = await _db.Evaluations.FirstOrDefaultAsync(x => x.Id == id);

        if (entity != null)

            _db.Evaluations.Remove(entity);

    }



    public Task SaveChangesAsync() => _db.SaveChangesAsync();



    private sealed class ClinicalCaseDiagnosisRaw

    {

        public string? CaseId { get; set; }

        public string? EccId { get; set; }

        public string? CanonicalDiagnosis { get; set; }

        public string? DescriptionText { get; set; }

        public string? Symptom { get; set; }

        public string? MedicalHistory { get; set; }

    }



    private sealed class VirtualPatientRaw

    {

        public string? PatientId { get; set; }

        public int? TimeSettingMinutes { get; set; }

        public int? ArgumentTimeMinutes { get; set; }

    }



    private sealed class RubricRaw

    {

        public string? Id { get; set; }

        public string? Description { get; set; }

        public string? Version { get; set; }

    }



    private sealed class IssueRow

    {

        public string? IssueId { get; set; }

        public string? LearnerId { get; set; }

        public string? LearnerName { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? Label { get; set; }

        public string? Description { get; set; }

        public string? Status { get; set; }

        public string? ExpertId { get; set; }

        public string? ExpertName { get; set; }

        public string? Feedback { get; set; }

    }



    private sealed class PracticeHistoryRaw

    {

        public string? PracticeSessionId { get; set; }

        public int? AttemptNo { get; set; }

        public string? EvaluationId { get; set; }

        public decimal? Score { get; set; }

        public int? PureEpaScore { get; set; }

        public int? EntrustmentLevel { get; set; }

        public string? FinalDiagnosis { get; set; }

        public int? Duration { get; set; }

        public string? DiagnosisMatch { get; set; }

        public string? RubricVersion { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? Status { get; set; }

        public string? FeedbackId { get; set; }

    }

}

" src\Services\EvaluationService\EvaluationService.Infrastructure\Rubrics\RubricProvider.cs:"using EvaluationService.Domain.Repositories;

using EvaluationService.Domain.Services;

using EvaluationService.Domain.ValueObjects;

using Microsoft.Extensions.Caching.Memory;

using Microsoft.Extensions.Logging;



namespace EvaluationService.Infrastructure.Rubrics;



// Load rubric từ evaluation_clinical_criteria.description

// Cache với IMemoryCache — rubric không đổi theo request

//   1. Kiểm tra memory cache => hit => return now

//   2. Miss => query DB => cache 1 giờ => return

//   3. DB trả null => return RubricContext.Empty

public sealed class RubricProvider : IRubricProvider

{

    private readonly IEvaluationRepository _repo;

    private readonly IMemoryCache _cache;

    private readonly ILogger<RubricProvider> _logger;



    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);



    public RubricProvider(

        IEvaluationRepository repo,

        IMemoryCache cache,

        ILogger<RubricProvider> logger

    )

    {

        _repo = repo;

        _cache = cache;

        _logger = logger;

    }



    public async Task<RubricContext> GetRubricAsync(string eccId, CancellationToken ct = default)

    {

        if (string.IsNullOrWhiteSpace(eccId))

        {

            _logger.LogWarning("GetRubricAsync called with empty eccId — returning empty rubric.");

            return RubricContext.Empty(eccId ?? string.Empty);

        }



        var cacheKey = $"rubric:{eccId}";



        if (_cache.TryGetValue(cacheKey, out RubricContext? cached) && cached != null)

            return cached;



        try

        {

            var rubricDto = await _repo.GetRubricByEccIdAsync(eccId);



            if (rubricDto == null)

            {

                _logger.LogWarning(

                    "Rubric not found for eccId={EccId}. Using empty rubric.",

                    eccId

                );

                var empty = RubricContext.Empty(eccId);

                _cache.Set(cacheKey, empty, TimeSpan.FromMinutes(10));

                return empty;

            }



            var context = new RubricContext(

                EccId: rubricDto.Id,

                Version: rubricDto.Version,

                FullContent: rubricDto.Description,

                IsAvailable: !string.IsNullOrWhiteSpace(rubricDto.Description)

            );



            _cache.Set(cacheKey, context, CacheDuration);

            _logger.LogInformation(

                "Rubric loaded: eccId={EccId} version={Version} contentLen={Len}",

                eccId,

                rubricDto.Version,

                rubricDto.Description?.Length ?? 0

            );



            return context;

        }

        catch (Exception ex)

        {

            _logger.LogError(ex, "Failed to load rubric for eccId={EccId}", eccId);

            return RubricContext.Empty(eccId);

        }

    }

}

" src\Services\EvaluationService\EvaluationService.Infrastructure\DependencyInjection.cs:"using EvaluationService.Domain.Repositories;

using EvaluationService.Domain.Services;

using EvaluationService.Infrastructure.Repositories;

using EvaluationService.Infrastructure.Rubrics;

using Microsoft.Extensions.Caching.Memory;

using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.DependencyInjection;



namespace EvaluationService.Infrastructure;



public static class DependencyInjection

{

    public static IServiceCollection AddInfrastructure(

        this IServiceCollection services,

        IConfiguration configuration

    )

    {

        services.AddScoped<IEvaluationRepository, EvaluationRepository>();



        services.AddHttpClient<GeminiEvaluationRepository>();

        services.AddScoped<IAiEvaluationProvider, GeminiEvaluationRepository>();



        services.AddMemoryCache();

        services.AddScoped<IRubricProvider, RubricProvider>();

        services.AddScoped<IEvaluationPromptBuilder, EvaluationPromptBuilder>();

        services.AddScoped<IFeedbackPromptBuilder, FeedbackPromptBuilder>();

        services.AddHttpClient();



        return services;

    }

}

" src\Services\EvaluationService\EvaluationService.Application\DependencyInjection.cs:"using System.Reflection;

using EvaluationService.Application.Orchestrators;

using EvaluationService.Application.Services;

using EvaluationService.Domain.Services;

using FluentValidation;

using MediatR;

using Microsoft.Extensions.DependencyInjection;



namespace EvaluationService.Application;



public static class DependencyInjection

{

    public static IServiceCollection AddApplication(this IServiceCollection services)

    {

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg =>

            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())

        );



        services.AddScoped<EvaluationOrchestrator>();

        services.AddScoped<IEpaScoreAggregator, EpaScoreAggregator>();

        services.AddScoped<IFeedbackComposer, FeedbackComposer>();

        services.AddScoped<IEvaluationPersistenceService, EvaluationPersistenceService>();



        return services;

    }

}

" 