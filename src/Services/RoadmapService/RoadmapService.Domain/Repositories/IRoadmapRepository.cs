using RoadmapService.Domain.Entities;

namespace RoadmapService.Domain.Repositories;

public interface IRoadmapRepository
{
    Task<Roadmap?> GetRoadmapByIdAsync(string roadmapId);

    Task<Roadmap?> GetLatestRoadmapAsync(string learnerId);

    Task<Roadmap> CreateRoadmapAsync(Roadmap roadmap);

    Task<Roadmap?> UpdateRoadmapContentAsync(string roadmapId, string contentJson);
}
