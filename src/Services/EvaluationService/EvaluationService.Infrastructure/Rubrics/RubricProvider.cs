using EvaluationService.Domain.Repositories;
using EvaluationService.Domain.Services;
using EvaluationService.Domain.ValueObjects;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EvaluationService.Infrastructure.Rubrics;

// Load rubric từ evaluation_clinical_criteria.description
// Cache với IMemoryCache — rubric không đổi theo request
//   1. Kiểm tra memory cache => hit => return now
//   2. Miss => query DB => cache 1 giờ => return
//   3. DB trả null => return RubricContext.Empty 
public sealed class RubricProvider : IRubricProvider
{
    private readonly IEvaluationRepository _repo;
    private readonly IMemoryCache          _cache;
    private readonly ILogger<RubricProvider> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public RubricProvider(
        IEvaluationRepository    repo,
        IMemoryCache             cache,
        ILogger<RubricProvider>  logger)
    {
        _repo   = repo;
        _cache  = cache;
        _logger = logger;
    }

    public async Task<RubricContext> GetRubricAsync(string eccId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(eccId))
        {
            _logger.LogWarning("GetRubricAsync called with empty eccId — returning empty rubric.");
            return RubricContext.Empty(eccId ?? string.Empty);
        }

        var cacheKey = $"rubric:{eccId}";

        if (_cache.TryGetValue(cacheKey, out RubricContext? cached) && cached != null)
            return cached;

        try
        {
            var rubricDto = await _repo.GetRubricByEccIdAsync(eccId);

            if (rubricDto == null)
            {
                _logger.LogWarning("Rubric not found for eccId={EccId}. Using empty rubric.", eccId);
                var empty = RubricContext.Empty(eccId);
                _cache.Set(cacheKey, empty, TimeSpan.FromMinutes(10)); 
                return empty;
            }

            var context = new RubricContext(
                EccId:       rubricDto.Id,
                Version:     rubricDto.Version,
                FullContent: rubricDto.Description,
                IsAvailable: !string.IsNullOrWhiteSpace(rubricDto.Description)
            );

            _cache.Set(cacheKey, context, CacheDuration);
            _logger.LogInformation(
                "Rubric loaded: eccId={EccId} version={Version} contentLen={Len}",
                eccId, rubricDto.Version, rubricDto.Description?.Length ?? 0);

            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load rubric for eccId={EccId}", eccId);
            return RubricContext.Empty(eccId);
        }
    }
}