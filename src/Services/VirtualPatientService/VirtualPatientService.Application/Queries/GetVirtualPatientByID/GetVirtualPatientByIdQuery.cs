using MediatR;
using VirtualPatientService.Application.Queries.GetVirtualPatients;

namespace VirtualPatientService.Application.Queries.GetVirtualPatientByID;

public record GetVirtualPatientByIdQuery(string PatientId) : IRequest<VirtualPatientDto?>;