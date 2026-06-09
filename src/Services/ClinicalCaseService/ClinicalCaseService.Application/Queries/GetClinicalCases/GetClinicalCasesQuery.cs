using ClinicalCaseService.Application.Queries.GetClinicalCases;
using MediatR;

public record GetClinicalCasesQuery(
    string? Search,
    string? Status,
    string? CaseType,
    string? EccId,
    string? SortBy,
    string? SortDir,
    int Page,
    int PageSize
) : IRequest<PagedResult<ClinicalCaseDto>>;
