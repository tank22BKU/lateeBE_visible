using MediatR;

namespace PracticeSessionService.Application.Queries.GetActivePracticeSession;

public class GetActivePracticeSessionRequest : IRequest<GetActivePracticeSessionResponse?>
{
    public string LearnerId { get; set; } = default!;
    public string PatientId { get; set; } = default!;
}
