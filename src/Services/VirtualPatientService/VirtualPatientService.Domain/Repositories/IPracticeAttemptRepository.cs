namespace VirtualPatientService.Domain.Repositories;

public interface IPracticeAttemptRepository
{
    Task<Dictionary<string, AttemptSummaryProjection>> GetAttemptSummariesAsync(
        string learnerId,
        IEnumerable<string> patientIds,
        CancellationToken cancellationToken = default);
}

public record AttemptSummaryProjection(
    bool Attempted,
    int AttemptCount,
    decimal? BestScore,
    decimal? LatestScore
);