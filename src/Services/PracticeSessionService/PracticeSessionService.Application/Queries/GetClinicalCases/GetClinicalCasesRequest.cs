using MediatR;

namespace PracticeSessionService.Application.Queries.GetClinicalCases;

public class GetClinicalCasesRequest : IRequest<PagedResult<ClinicalCaseDto>>
{
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
