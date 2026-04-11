using MediatR;

namespace VirtualPatientService.Application.Queries.GetVirtualPatients;

public record GetVirtualPatientQuery(
    string? Gender, 
    int Page,
    int PageSize
) : IRequest<PagedResult<VirtualPatientDto>>;