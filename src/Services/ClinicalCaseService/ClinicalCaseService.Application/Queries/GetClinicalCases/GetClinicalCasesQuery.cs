using MediatR;

using ClinicalCaseService.Application.Queries.GetClinicalCases;

public record GetClinicalCasesQuery(
    string? Status,
    int Page,
    int PageSize
) : IRequest<PagedResult<ClinicalCaseDto>>;
