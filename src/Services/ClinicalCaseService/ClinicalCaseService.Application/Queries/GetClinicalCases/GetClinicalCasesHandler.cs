using ClinicalCaseService.Domain.Repositories;
using ClinicalCaseService.Application.Queries.GetClinicalCases;
using MediatR;

namespace ClinicalCaseService.Application.Queries.GetClinicalCases;

public class GetClinicalCasesHandler : IRequestHandler<GetClinicalCasesQuery, PagedResult<ClinicalCaseDto>>
{
    private readonly IClinicalCaseRepository _repo;

    public GetClinicalCasesHandler(IClinicalCaseRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<ClinicalCaseDto>> Handle(GetClinicalCasesQuery q, CancellationToken cancellationToken)
    {
        if (q.Page < 1) q = q with { Page = 1 };
        if (q.PageSize <= 0 || q.PageSize > 100)
            q = q with { PageSize = 20 };

        var (items, total) =
            await _repo.GetPagedAsync(q.Status, q.Page, q.PageSize);

        return new PagedResult<ClinicalCaseDto>
        {
            Items = items.Select(x => new ClinicalCaseDto
            {
                CaseId = x.CaseId,
                Title = x.Title,
                Description = x.Description,
                CaseType = x.CaseType,
                Status = x.Status,
                Pe = x.Pe,
                Symptom = x.Symptom,
                MedicalHistory = x.MedicalHistory,
                CreatedBy = x.CreatedBy,
                EccId = x.EccId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToList(),
            Total = total,
            Page = q.Page,
            PageSize = q.PageSize
        };
    }
}

