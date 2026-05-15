using MediatR;
using PracticeSessionService.Domain.Entities.Constants;
using PracticeSessionService.Domain.Repositories;

namespace PracticeSessionService.Application.Queries.GetAttemptCount;

public class GetAttemptCountHandler
	: IRequestHandler<GetAttemptCountRequest, GetAttemptCountResponse>
{
	private readonly IPracticeSessionRepository _repo;

	public GetAttemptCountHandler(IPracticeSessionRepository repo)
	{
		_repo = repo;
	}

	public async Task<GetAttemptCountResponse> Handle(
		GetAttemptCountRequest request,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.LearnerId))
			throw new ArgumentException("LearnerId is required.");
		if (string.IsNullOrWhiteSpace(request.PatientId))
			throw new ArgumentException("PatientId is required.");

		var attemptCount = await _repo.CountSessionsAsync(
			request.LearnerId,
			request.PatientId,
			PracticeSessionStatuses.AttemptStatuses);

		var activeSession = await _repo.GetLatestSessionAsync(
			request.LearnerId,
			request.PatientId,
			PracticeSessionStatuses.ActiveStatuses);

		var canAttempt = attemptCount < PracticeSessionRules.MaxAttempts
						 && activeSession == null;

		return new GetAttemptCountResponse
		{
			LearnerId = request.LearnerId,
			PatientId = request.PatientId,
			AttemptCount = attemptCount,
			MaxAttempts = PracticeSessionRules.MaxAttempts,
			CanAttempt = canAttempt
		};
	}
}