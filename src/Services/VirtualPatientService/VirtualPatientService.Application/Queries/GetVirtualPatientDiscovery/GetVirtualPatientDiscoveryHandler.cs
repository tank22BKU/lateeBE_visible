using MediatR;
using VirtualPatientService.Application.Dtos;
using VirtualPatientService.Domain.Constants;
using VirtualPatientService.Domain.Repositories;

namespace VirtualPatientService.Application.Queries.GetVirtualPatientDiscovery;

public class GetVirtualPatientDiscoveryHandler
    : IRequestHandler<GetVirtualPatientDiscoveryQuery, GetVirtualPatientDiscoveryResponse>
{
    private readonly ILearnerDiscoveryPoolRepository _poolRepo;

    public GetVirtualPatientDiscoveryHandler(ILearnerDiscoveryPoolRepository poolRepo)
    {
        _poolRepo = poolRepo;
    }

    public async Task<GetVirtualPatientDiscoveryResponse> Handle(
        GetVirtualPatientDiscoveryQuery request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.LearnerId))
            throw new ArgumentException("learnerId is required");

        var sortBy = NormalizeSortBy(request.SortBy);

        var poolItems = await _poolRepo.GetPoolItemsAsync(
            request.LearnerId,
            sortBy,
            cancellationToken
        );

        if (poolItems.Count == 0)
        {
            return BuildEmptyResponse(1, request.PageSize);
        }

        var items = poolItems
            .Select(x =>
            {
                return new VirtualPatientDiscoveryItemDto
                {
                    PatientId = x.PatientId,
                    CaseId = x.CaseId,
                    Name = x.Name,
                    Age = x.Age,
                    Gender = x.Gender,
                    Occupation = x.Occupation,
                    ChiefConcern = x.ChiefConcern,
                    Symptom = x.Symptom,
                    Level = x.Level,
                    AvatarImage = x.AvatarImage,
                    TimeSetting = x.TimeSetting,
                    ArgumentTime = x.ArgumentTime,
                    CreatedAt = x.CreatedAt,
                    FeedbackCount = x.AttemptSummary.AttemptCount,
                    AttemptSummary = new AttemptSummaryDto
                    {
                        Attempted = x.AttemptSummary.Attempted,
                        AttemptCount = x.AttemptSummary.AttemptCount,
                        MaxAttempts = VirtualPatientConstants.MaxAttemptsAllowed,
                        BestScore = x.AttemptSummary.BestScore,
                        LatestScore = x.AttemptSummary.LatestScore,
                    },
                    Experts = x
                        .Experts.Select(e => new ExpertPreviewDto
                        {
                            ExpertId = e.ExpertId,
                            Name = e.Name,
                            Role = e.Role,
                            AvatarUrl = e.AvatarUrl,
                        })
                        .ToList(),
                };
            })
            .ToList();

        return new GetVirtualPatientDiscoveryResponse
        {
            Items = items,
            Total = poolItems.Count,
            Page = 1,
            PageSize = request.PageSize,
            Filters = new DiscoveryFiltersDto
            {
                AvailableLevels = poolItems
                    .Select(x => x.Level)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList()!,
                AvailableGenders = poolItems
                    .Select(x => x.Gender)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList()!,
                AvailableSpecialties = new(),
                AvailableCaseTypes = new(),
            },
        };
    }

    private static string NormalizeSortBy(string? sortBy) =>
        !string.IsNullOrWhiteSpace(sortBy)
        && VirtualPatientConstants.SortOptions.AllowedSorts.Contains(sortBy)
            ? sortBy
            : VirtualPatientConstants.SortOptions.Newest;

    private static GetVirtualPatientDiscoveryResponse BuildEmptyResponse(int page, int pageSize) =>
        new()
        {
            Items = new(),
            Total = 0,
            Page = page,
            PageSize = pageSize,
            Filters = new DiscoveryFiltersDto
            {
                AvailableLevels = new(),
                AvailableGenders = new(),
                AvailableSpecialties = new(),
                AvailableCaseTypes = new(),
            },
        };
}
