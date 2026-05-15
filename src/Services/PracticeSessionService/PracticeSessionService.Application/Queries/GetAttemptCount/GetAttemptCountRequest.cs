using MediatR;

namespace PracticeSessionService.Application.Queries.GetAttemptCount;

public class GetAttemptCountRequest : IRequest<GetAttemptCountResponse>
{
	public string LearnerId { get; set; } = default!;
	public string PatientId { get; set; } = default!;
}