using MediatR;

namespace PracticeSessionService.Application.Commands.CreatePracticeSession;

public class CreatePracticeSessionCommand : IRequest<CreatePracticeSessionResult>
{
    public string? Id { get; set; } 
    public string? LearnerId { get; set; }
    public string? ClinicalCaseId { get; set; }
    public string? Status { get; set; } = "Practicing";
}