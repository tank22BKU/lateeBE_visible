using MediatR;
using PracticeSessionService.Domain.Entities.Constants;
using PracticeSessionService.Domain.Repositories;

namespace PracticeSessionService.Application.Commands.UpdatePracticeSessionStatus;

public class UpdatePracticeSessionStatusHandler
    : IRequestHandler<UpdatePracticeSessionStatusCommand, UpdatePracticeSessionStatusResponse?>
{
    private readonly IPracticeSessionRepository _repo;

    public UpdatePracticeSessionStatusHandler(IPracticeSessionRepository repo)
    {
        _repo = repo;
    }

    public async Task<UpdatePracticeSessionStatusResponse?> Handle(
        UpdatePracticeSessionStatusCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
            throw new ArgumentException("SessionId is required.");
        if (string.IsNullOrWhiteSpace(request.Status))
            throw new ArgumentException("Status is required.");

        var normalizedStatus = request.Status.Trim();
        if (!PracticeSessionStatuses.AllStatuses.Contains(normalizedStatus))
            throw new ArgumentException("Invalid status value.");

        var session = await _repo.GetSessionByIdAsync(request.SessionId);
        if (session == null) return null;

        if (string.Equals(session.Status, normalizedStatus, StringComparison.OrdinalIgnoreCase))
        {
            return new UpdatePracticeSessionStatusResponse
            {
                SessionId = session.Id,
                Status = session.Status
            };
        }

        if (string.Equals(session.Status, PracticeSessionStatuses.Completed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(session.Status, PracticeSessionStatuses.Abandoned, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cannot change status of a completed or abandoned session.");
        }

        session.Status = normalizedStatus;
        if (string.Equals(normalizedStatus, PracticeSessionStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            session.EndTime = DateTime.UtcNow;
        }

        await _repo.UpdateSessionAsync(session);
        await _repo.SaveChangesAsync();

        return new UpdatePracticeSessionStatusResponse
        {
            SessionId = session.Id,
            Status = session.Status
        };
    }
}
