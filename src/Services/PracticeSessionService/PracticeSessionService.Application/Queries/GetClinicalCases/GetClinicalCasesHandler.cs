using MediatR;
using PracticeSessionService.Domain.Repositories;

namespace PracticeSessionService.Application.Queries.GetClinicalCases;

public class GetClinicalCasesHandler
    : IRequestHandler<GetClinicalCasesRequest, PagedResult<ClinicalCaseDto>>
{
    private readonly IClinicalCaseRepository _repo;

    public GetClinicalCasesHandler(IClinicalCaseRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<ClinicalCaseDto>> Handle(
        GetClinicalCasesRequest request,
        CancellationToken cancellationToken
    )
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 || request.PageSize > 100 ? 20 : request.PageSize;

        var (items, total) = await _repo.GetPagedAsync(request.Status, page, pageSize);

        return new PagedResult<ClinicalCaseDto>
        {
            Items = items
                .Select(x => new ClinicalCaseDto
                {
                    Id = x.CaseId,
                    Title = x.Title,
                    Type = x.Type,
                    Status = x.Status,
                })
                .ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize,
        };
    }
}
