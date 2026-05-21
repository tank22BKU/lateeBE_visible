using Microsoft.EntityFrameworkCore;
using VirtualPatientService.Domain.Constants;
using VirtualPatientService.Domain.Repositories;
using VirtualPatientService.Infrastructure.Persistance;

namespace VirtualPatientService.Infrastructure.Repositories;

public class PracticeAttemptRepository : IPracticeAttemptRepository
{
    private readonly VirtualPatientDbContext _db;

    public PracticeAttemptRepository(VirtualPatientDbContext db) => _db = db;

    public async Task<Dictionary<string, AttemptSummaryProjection>> GetAttemptSummariesAsync(
        string learnerId,
        IEnumerable<string> patientIds,
        CancellationToken cancellationToken = default
    )
    {
        var ids = patientIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<string, AttemptSummaryProjection>();

        var raw = await (
            from ps in _db.PracticeSessionRefs.AsNoTracking()
            where
                ps.LearnerId == learnerId
                && ids.Contains(ps.PatientId)
                && (
                    ps.Status == VirtualPatientConstants.PracticeStatus.Practicing
                    || ps.Status == VirtualPatientConstants.PracticeStatus.Completed
                )
            join e in _db.EvaluationRefs.AsNoTracking()
                on ps.Id equals e.PracticeSessionId
                into evals
            from e in evals.DefaultIfEmpty()
            select new
            {
                ps.PatientId,
                ps.CreatedAt,
                Score = e != null ? e.Score : (decimal?)null,
            }
        ).ToListAsync(cancellationToken);

        return raw.GroupBy(a => a.PatientId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var attempts = g.ToList();
                    var scores = attempts
                        .Where(a => a.Score.HasValue)
                        .Select(a => a.Score!.Value)
                        .ToList();

                    var latestScore = attempts
                        .OrderByDescending(a => a.CreatedAt)
                        .FirstOrDefault()
                        ?.Score;

                    return new AttemptSummaryProjection(
                        Attempted: attempts.Count > 0,
                        AttemptCount: attempts.Count,
                        BestScore: scores.Count > 0 ? scores.Max() : null,
                        LatestScore: latestScore
                    );
                }
            );
    }
}
