using MediatR;
using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using EvaluationService.Application.Interfaces;

namespace EvaluationService.Application.Commands.SubmitEvaluation;

public record SubmitEvaluationCommand(
    string UserId, string ClinicalCaseId, string VpLog, string ReasoningLog, 
    string Diagnosis, List<WarningDto> Warnings) : IRequest<string>;

public class SubmitEvaluationHandler : IRequestHandler<SubmitEvaluationCommand, string> {
    private readonly IEvaluationRepository _repo;
    private readonly IAIEvaluatorService _ai;

    public SubmitEvaluationHandler(IEvaluationRepository repo, IAIEvaluatorService ai) {
        _repo = repo; _ai = ai;
    }

    public async Task<string> Handle(SubmitEvaluationCommand request, CancellationToken ct) {
        var result = new EvaluationResult {
            UserId = request.UserId, ClinicalCaseId = request.ClinicalCaseId,
            VpConversationLog = request.VpLog, AiReasoningLog = request.ReasoningLog,
            FinalDiagnosis = request.Diagnosis
        };

        result.Epas = await _ai.AnalyzePerformanceAsync(result);
        
        await _repo.AddAsync(result);
        await _repo.SaveChangesAsync();
        return result.ResultId;
    }
}