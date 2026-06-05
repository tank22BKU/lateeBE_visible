using RoadmapService.Domain.Entities;
using RoadmapService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RoadmapService.Domain.Repositories;

namespace RoadmapService.Infrastructure.Repositories;

public class RoadmapRepository : IRoadmapRepository
{
    private readonly RoadmapDbContext _db;

    public RoadmapRepository(RoadmapDbContext db)
    {
        _db = db;
    }
    
    public Task<Roadmap?> GetRoadmapByIdAsync(string roadmapId)
    {
        return _db.Roadmaps
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RoadmapId == roadmapId);
    }

    public Task<Roadmap?> GetLatestRoadmapAsync(string learnerId)
    {
        return _db.Roadmaps
            .AsNoTracking()
            .Where(x => x.LearnerId == learnerId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<UnsummarizedEvaluationHistoryRow>> GetUnsummarizedEvaluationHistoryAsync(
        string learnerId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _db.Database.SqlQuery<UnsummarizedEvaluationHistoryRaw>(
                $"""
                SELECT
                    t.EvaluationId AS EvaluationId,
                    t.PracticeSessionId AS PracticeSessionId,
                    t.PracticeSessionCreatedAt AS PracticeSessionCreatedAt,
                    t.EvaluationCreatedAt AS EvaluationCreatedAt,
                    t.FeedbackDetail AS FeedbackDetail
                FROM (
                    SELECT
                        e.id AS EvaluationId,
                        e.practice_session_id AS PracticeSessionId,
                        ps.created_at AS PracticeSessionCreatedAt,
                        e.created_at AS EvaluationCreatedAt,
                        COALESCE(e.feedback_detail, '') AS FeedbackDetail,
                        ROW_NUMBER() OVER (
                            PARTITION BY ps.id
                            ORDER BY e.created_at DESC, e.id DESC
                        ) AS RowNumber
                    FROM evaluation e
                    INNER JOIN practice_sessions ps ON ps.id = e.practice_session_id
                    LEFT JOIN summarize_roadmap sr ON sr.evaluation_id = e.id
                    WHERE ps.learner_id = {learnerId}
                        AND sr.evaluation_id IS NULL
                ) t
                WHERE t.RowNumber = 1
                ORDER BY t.PracticeSessionCreatedAt DESC, t.EvaluationCreatedAt DESC
                """
            )
            .ToListAsync(cancellationToken);

        return rows.Select(r => new UnsummarizedEvaluationHistoryRow(
                EvaluationId: r.EvaluationId ?? string.Empty,
                PracticeSessionId: r.PracticeSessionId ?? string.Empty,
                PracticeSessionCreatedAt: r.PracticeSessionCreatedAt ?? DateTime.UtcNow,
                EvaluationCreatedAt: r.EvaluationCreatedAt ?? DateTime.UtcNow,
                FeedbackDetail: r.FeedbackDetail ?? string.Empty))
            .ToList();
    }

    public async Task<Roadmap> CreateRoadmapAsync(Roadmap roadmap)
    {
        _db.Roadmaps.Add(roadmap);
        await _db.SaveChangesAsync();
        return roadmap;
    }

    public async Task AddSummarizeRoadmapsAsync(
        string roadmapId,
        IEnumerable<string> evaluationIds,
        CancellationToken cancellationToken = default)
    {
        var summarized = evaluationIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(x => new SummarizeRoadmap
            {
                RoadmapId = roadmapId,
                EvaluationId = x
            })
            .ToList();

        if (summarized.Count == 0)
        {
            return;
        }

        _db.SummarizeRoadmaps.AddRange(summarized);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Roadmap?> UpdateRoadmapContentAsync(string roadmapId, string contentJson)
    {
        var roadmap = await _db.Roadmaps.FirstOrDefaultAsync(x => x.RoadmapId == roadmapId);

        if (roadmap is null)
        {
            return null;
        }

        roadmap.Content = contentJson;
        await _db.SaveChangesAsync();

        return roadmap;
    }

    private sealed class UnsummarizedEvaluationHistoryRaw
    {
        public string? EvaluationId { get; set; }

        public string? PracticeSessionId { get; set; }

        public DateTime? PracticeSessionCreatedAt { get; set; }

        public DateTime? EvaluationCreatedAt { get; set; }

        public string? FeedbackDetail { get; set; }
    }

}