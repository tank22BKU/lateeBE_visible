using MediatR;
using PracticeSessionService.Domain.Entities.Constants;
using PracticeSessionService.Domain.Repositories;

namespace PracticeSessionService.Application.Queries.GetActivePracticeSession;

public class GetActivePracticeSessionHandler
	: IRequestHandler<GetActivePracticeSessionRequest, GetActivePracticeSessionResponse?>
{
	private readonly IPracticeSessionRepository _repo;

	public GetActivePracticeSessionHandler(IPracticeSessionRepository repo)
	{
		_repo = repo;
	}

	public async Task<GetActivePracticeSessionResponse?> Handle(
		GetActivePracticeSessionRequest request,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.LearnerId))
			throw new ArgumentException("LearnerId is required.");
		if (string.IsNullOrWhiteSpace(request.PatientId))
			throw new ArgumentException("PatientId is required.");

		var session = await _repo.GetLatestSessionAsync(
			request.LearnerId,
			request.PatientId,
			PracticeSessionStatuses.ActiveStatuses);

		if (session == null) return null;

		return new GetActivePracticeSessionResponse
		{
			SessionId = session.Id,
			Status = session.Status,
			StartTime = session.StartTime,
			PatientId = session.PatientId
		};
	}
}