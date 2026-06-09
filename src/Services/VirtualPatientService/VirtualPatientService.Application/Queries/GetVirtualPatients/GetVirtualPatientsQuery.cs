using MediatR;
using VirtualPatientService.Application.Dtos;

namespace VirtualPatientService.Application.Queries.GetVirtualPatients;

public record GetVirtualPatientsQuery(string? Gender, int Page, int PageSize)
    : IRequest<PageResult<VirtualPatientListItemDto>>;
