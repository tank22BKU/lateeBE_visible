using MediatR;

namespace PracticeSessionService.Application.Commands.UpdatePracticeSessionStatus;

public class UpdatePracticeSessionStatusCommand : IRequest<UpdatePracticeSessionStatusResponse?>
{
    public string SessionId { get; set; } = default!;
    public string Status { get; set; } = default!;
}
