using MediatR;

using RoadmapService.Application.Queries.GetClinicalCases;

namespace RoadmapService.Application.Queries.GetClinicalCases;
public record GetClinicalCasesQuery(
    string? Status,
    int Page,
    int PageSize
) : IRequest<PagedResult<ClinicalCaseDto>>;
