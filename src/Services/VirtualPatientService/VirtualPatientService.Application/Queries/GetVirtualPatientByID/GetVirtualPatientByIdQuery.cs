using MediatR;
using VirtualPatientService.Application.Dtos;

namespace VirtualPatientService.Application.Queries.GetVirtualPatientById;

public record GetVirtualPatientByIdQuery(string PatientId) : IRequest<VirtualPatientDetailDto?>;
