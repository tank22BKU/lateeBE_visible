using MediatR;

namespace PracticeSessionService.Application.Queries.GetPracticeSessions;

public class GetPracticeSessionsRequest : IRequest<GetPracticeSessionsResponse>
{
    public string SessionId { get; set; } = default!;
}