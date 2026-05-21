using Microsoft.EntityFrameworkCore;
using VirtualPatientService.Domain.Entities;
using VirtualPatientService.Domain.Repositories;
using VirtualPatientService.Infrastructure.Persistance;

namespace VirtualPatientService.Infrastructure.Repositories;

public class LearnerDiscoveryStateRepository : ILearnerDiscoveryStateRepository
{
    private readonly VirtualPatientDbContext _db;

    public LearnerDiscoveryStateRepository(VirtualPatientDbContext db) => _db = db;

    public Task<LearnerDiscoveryState?> GetByLearnerIdAsync(
        string learnerId,
        CancellationToken cancellationToken = default
    ) =>
        _db
            .LearnerDiscoveryStates.AsNoTracking()
            .FirstOrDefaultAsync(x => x.LearnerId == learnerId, cancellationToken);

    public async Task UpsertAsync(
        LearnerDiscoveryState state,
        CancellationToken cancellationToken = default
    )
    {
        var updated = await _db
            .LearnerDiscoveryStates.Where(x => x.LearnerId == state.LearnerId)
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(x => x.FilterJson, state.FilterJson)
                        .SetProperty(x => x.LastAccessed, state.LastAccessed),
                cancellationToken
            );

        if (updated == 0)
        {
            try
            {
                _db.LearnerDiscoveryStates.Add(state);
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                await _db
                    .LearnerDiscoveryStates.Where(x => x.LearnerId == state.LearnerId)
                    .ExecuteUpdateAsync(
                        s =>
                            s.SetProperty(x => x.FilterJson, state.FilterJson)
                                .SetProperty(x => x.LastAccessed, state.LastAccessed),
                        cancellationToken
                    );
            }
        }
    }
}
