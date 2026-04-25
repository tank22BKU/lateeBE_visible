using MediatR;

using PracticeSessionService.Application.Queries.GetClinicalCases;

public record GetPracticeSessionsQuery(
    string? Status,
    int Page,
    int PageSize
) : IRequest<PagedResult<PracticeSessionDto>>;
