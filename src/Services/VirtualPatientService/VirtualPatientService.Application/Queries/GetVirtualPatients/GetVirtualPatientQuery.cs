using MediatR;

using VirtualPatientService.Application.Queries.GetVirtualPatients;

public record GetVirtualPatientQuery(
    char? Gender,
    int Page,
    int PageSize
) : IRequest<PagedResult<VirtualPatientDto>>;
