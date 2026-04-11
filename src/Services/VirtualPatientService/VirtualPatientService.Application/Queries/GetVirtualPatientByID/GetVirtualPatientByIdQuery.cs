using System.Net;
using MediatR;

namespace VirtualPatientService.Application.Queries.GetVirtualPatients;

public record GetVirtualPatientByIdQuery(string PatientId) : IRequest<VirtualPatientDto?>;