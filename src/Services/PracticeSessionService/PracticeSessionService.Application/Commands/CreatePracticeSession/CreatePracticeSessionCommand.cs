using MediatR;

namespace PracticeSessionService.Application.Commands.CreatePracticeSession;

public class CreatePracticeSessionCommand : IRequest<CreatePracticeSessionResult>
{
    public string? Id { get; set; } 
    public string? LearnerId { get; set; }
    public string? PatientId { get; set; }
    public string? ModuleId { get; set; }
    public string? DiscussionType { get; set; }
    public string? GuidelinesId { get; set; }
    public string? Status { get; set; } = "Practicing";
}