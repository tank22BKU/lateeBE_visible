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

    public async Task<Roadmap> CreateRoadmapAsync(Roadmap roadmap)
    {
        _db.Roadmaps.Add(roadmap);
        await _db.SaveChangesAsync();
        return roadmap;
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

}