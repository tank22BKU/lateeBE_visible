using MediatR;

namespace EvaluationService.Application.Commands.GeneratePracticeSession;

public class GeneratePracticeSessionCommand : IRequest<GeneratePracticeSessionResult>
{
    public string? Id { get; set; }
    public string? LearnerId { get; set; }
    public string? ClinicalCaseId { get; set; }
    public string? Status { get; set; } = "Practicing";
}
