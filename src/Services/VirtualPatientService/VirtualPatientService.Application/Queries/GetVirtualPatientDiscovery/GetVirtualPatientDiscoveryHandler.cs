using MediatR;
using VirtualPatientService.Application.Dtos;
using VirtualPatientService.Domain.Constants;
using VirtualPatientService.Domain.Repositories;

namespace VirtualPatientService.Application.Queries.GetVirtualPatientDiscovery;

public class GetVirtualPatientDiscoveryHandler
    : IRequestHandler<GetVirtualPatientDiscoveryQuery, GetVirtualPatientDiscoveryResponse>
{
    private readonly IVirtualPatientRepository _vpRepo;
    private readonly IPracticeAttemptRepository _attemptRepo;

    public GetVirtualPatientDiscoveryHandler(
        IVirtualPatientRepository vpRepo,
        IPracticeAttemptRepository attemptRepo
    )
    {
        _vpRepo = vpRepo;
        _attemptRepo = attemptRepo;
    }

    public async Task<GetVirtualPatientDiscoveryResponse> Handle(
        GetVirtualPatientDiscoveryQuery request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.LearnerId))
            throw new ArgumentException("learnerId is required");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize =
            request.PageSize <= 0 || request.PageSize > VirtualPatientConstants.MaxPageSize
                ? VirtualPatientConstants.DefaultDiscoveryPageSize
                : request.PageSize;

        var sortBy = NormalizeSortBy(request.SortBy);

        var (rawItems, total) = await _vpRepo.GetPagedForDiscoveryAsync(
            page,
            pageSize,
            request.Level,
            request.Occupation,
            request.ExpertId,
            request.Gender,
            request.Specialty,
            request.CaseType,
            request.Search,
            sortBy,
            cancellationToken
        );

        if (rawItems.Count == 0)
        {
            var emptyFilters = await _vpRepo.GetDiscoveryFiltersAsync(cancellationToken);
            return BuildEmptyResponse(page, pageSize, emptyFilters);
        }

        var patientIds = rawItems.Select(x => x.PatientId).Distinct().ToList();

        var expertsByPatient = await _vpRepo.GetExpertsByPatientIdsAsync(
            patientIds,
            cancellationToken
        );

        var attemptSummaries = await _attemptRepo.GetAttemptSummariesAsync(
            request.LearnerId,
            patientIds,
            cancellationToken
        );

        var filters = await _vpRepo.GetDiscoveryFiltersAsync(cancellationToken);

        var items = rawItems
            .Select(x =>
            {
                expertsByPatient.TryGetValue(x.PatientId, out var experts);
                attemptSummaries.TryGetValue(x.PatientId, out var attempt);

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
                    FeedbackCount = attempt?.AttemptCount ?? 0,
                    AttemptSummary = attempt is null
                        ? new AttemptSummaryDto
                        {
                            Attempted = false,
                            AttemptCount = 0,
                            MaxAttempts = VirtualPatientConstants.MaxAttemptsAllowed,
                            BestScore = null,
                            LatestScore = null,
                        }
                        : new AttemptSummaryDto
                        {
                            Attempted = attempt.Attempted,
                            AttemptCount = attempt.AttemptCount,
                            MaxAttempts = VirtualPatientConstants.MaxAttemptsAllowed,
                            BestScore = attempt.BestScore,
                            LatestScore = attempt.LatestScore,
                        },
                    Experts = (experts ?? new())
                        .Select(e => new ExpertPreviewDto
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
            Total = total,
            Page = page,
            PageSize = pageSize,
            Filters = new DiscoveryFiltersDto
            {
                AvailableLevels = filters.AvailableLevels,
                AvailableGenders = filters.AvailableGenders,
                AvailableSpecialties = filters.AvailableSpecialties,
                AvailableCaseTypes = filters.AvailableCaseTypes,
            },
        };
    }

    private static string NormalizeSortBy(string? sortBy) =>
        !string.IsNullOrWhiteSpace(sortBy)
        && VirtualPatientConstants.SortOptions.AllowedSorts.Contains(sortBy)
            ? sortBy
            : VirtualPatientConstants.SortOptions.Newest;

    private static GetVirtualPatientDiscoveryResponse BuildEmptyResponse(
        int page,
        int pageSize,
        DiscoveryFiltersProjection filters
    ) =>
        new()
        {
            Items = new(),
            Total = 0,
            Page = page,
            PageSize = pageSize,
            Filters = new DiscoveryFiltersDto
            {
                AvailableLevels = filters.AvailableLevels,
                AvailableGenders = filters.AvailableGenders,
                AvailableSpecialties = filters.AvailableSpecialties,
                AvailableCaseTypes = filters.AvailableCaseTypes,
            },
        };
}
