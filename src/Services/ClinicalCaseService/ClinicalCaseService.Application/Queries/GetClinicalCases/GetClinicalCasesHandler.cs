using ClinicalCaseService.Application.Queries.GetClinicalCases;
using ClinicalCaseService.Domain.Repositories;
using MediatR;

namespace ClinicalCaseService.Application.Queries.GetClinicalCases;

public class GetClinicalCasesHandler
    : IRequestHandler<GetClinicalCasesQuery, PagedResult<ClinicalCaseDto>>
{
    private readonly IClinicalCaseRepository _repo;

    private static readonly ClinicalCaseListFilters DefaultFilters = new()
    {
        AvailableStatuses = ["active", "draft", "archived", "published"],
        AvailableTypes = ["APPENDICITIS", "ABDOMINAL_PAIN", "CHEST_PAIN"],
        AvailableEccids = [],
    };

    public GetClinicalCasesHandler(IClinicalCaseRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<ClinicalCaseDto>> Handle(
        GetClinicalCasesQuery q,
        CancellationToken cancellationToken
    )
    {
        if (q.Page < 1)
            q = q with { Page = 1 };
        if (q.PageSize <= 0 || q.PageSize > 100)
            q = q with { PageSize = 20 };

        var (items, total) = await _repo.GetPagedAsync(
            q.Search,
            q.Status,
            q.CaseType,
            q.EccId,
            q.SortBy,
            q.SortDir,
            q.Page,
            q.PageSize
        );

        var itemDtos = new List<ClinicalCaseDto>(items.Count);

        foreach (var clinicalCase in items)
        {
            var stats = await _repo.GetStatsByCaseIdAsync(clinicalCase.CaseId);
            var createdByName = await _repo.GetExpertNameAsync(clinicalCase.CreatedBy);

            itemDtos.Add(
                new ClinicalCaseDto
                {
                    CaseId = clinicalCase.CaseId,
                    Title = clinicalCase.Title,
                    Description = clinicalCase.Description,
                    CaseType = clinicalCase.CaseType,
                    Status = clinicalCase.Status,
                    Pe = clinicalCase.Pe,
                    Symptom = clinicalCase.Symptom,
                    MedicalHistory = clinicalCase.MedicalHistory,
                    CreatedBy = clinicalCase.CreatedBy,
                    CreatedByName = createdByName,
                    EccId = clinicalCase.EccId,
                    CreatedAt = clinicalCase.CreatedAt,
                    UpdatedAt = clinicalCase.UpdatedAt,
                    VirtualPatientCount = stats?.VirtualPatientCount ?? 0,
                    AttemptCount = stats?.TotalAttempts ?? 0,
                    AvgScore = stats?.AvgScore ?? 0m,
                }
            );
        }

        return new PagedResult<ClinicalCaseDto>
        {
            Items = itemDtos,
            Total = total,
            Page = q.Page,
            PageSize = q.PageSize,
            Filters = DefaultFilters,
        };
    }
}
