using RoadmapService.Domain.Entities;

namespace RoadmapService.Domain.Repositories;

public interface IRoadmapRepository
{
    Task<Roadmap?> GetRoadmapByIdAsync(string roadmapId);

    Task<Roadmap?> GetLatestRoadmapAsync(string learnerId);

    Task<List<UnsummarizedEvaluationHistoryRow>> GetUnsummarizedEvaluationHistoryAsync(
        string learnerId,
        CancellationToken cancellationToken = default);

    Task<Roadmap> CreateRoadmapAsync(Roadmap roadmap);

    Task AddSummarizeRoadmapsAsync(
        string roadmapId,
        IEnumerable<string> evaluationIds,
        CancellationToken cancellationToken = default);

    Task<Roadmap?> UpdateRoadmapContentAsync(string roadmapId, string contentJson);
}

public record UnsummarizedEvaluationHistoryRow(
    string EvaluationId,
    string PracticeSessionId,
    DateTime PracticeSessionCreatedAt,
    DateTime EvaluationCreatedAt,
    string FeedbackDetail);
