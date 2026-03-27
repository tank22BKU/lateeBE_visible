using MediatR;
using System.Text.Json;
using VirtualPatientService.Domain.Repositories;

namespace VirtualPatientService.Application.Queries.GetVirtualPatients;

public class GetVirtualPatientByIdHandler : IRequestHandler<GetVirtualPatientByIdQuery, VirtualPatientDto?>
{
    private readonly IVirtualPatientRepository _repo;

    public GetVirtualPatientByIdHandler(IVirtualPatientRepository repo)
    {
        _repo = repo;
    }

    public async Task<VirtualPatientDto?> Handle(GetVirtualPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var x = await _repo.GetByIdAsync(request.PatientId);
        
        if (x == null) return null;

        return new VirtualPatientDto
        {
            PatientId = x.PatientId,
            ClinicalCaseId = x.ClinicalCaseId,
            Name = x.Name,
            Age = x.Age,
            Gender = x.Gender,
            Occupation = x.Occupation,
            Description = x.Description,
            ChiefConcern = x.ChiefConcern,
            VitalSigns = string.IsNullOrEmpty(x.VitalSigns) ? null : JsonSerializer.Deserialize<object>(x.VitalSigns),
            Instructions = string.IsNullOrEmpty(x.Instructions) ? null : JsonSerializer.Deserialize<object>(x.Instructions),
            CaseRules = string.IsNullOrEmpty(x.CaseRules) ? null : JsonSerializer.Deserialize<object>(x.CaseRules),
            Persona = string.IsNullOrEmpty(x.Persona) ? null : JsonSerializer.Deserialize<object>(x.Persona)
        };
    }
}