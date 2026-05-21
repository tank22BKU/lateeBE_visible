namespace PracticeSessionService.Application.Commands.UpdatePracticeSessionStatus;

public class UpdatePracticeSessionStatusResponse
{
    public string SessionId { get; set; } = default!;
    public string Status { get; set; } = default!;
}
