using System.Text.Json;
using MediatR;
using VirtualPatientService.Domain.Repositories;

namespace VirtualPatientService.Application.Queries.GetVirtualPatients;

public class GetVirtualPatientsHandler : IRequestHandler<GetVirtualPatientQuery, PagedResult<VirtualPatientDto>>
{
    private readonly IVirtualPatientRepository _repo;

    public GetVirtualPatientsHandler(IVirtualPatientRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<VirtualPatientDto>> Handle(GetVirtualPatientQuery q, CancellationToken cancellationToken)
    {
        if (q.Page < 1) q = q with { Page = 1 };
        if (q.PageSize <= 0 || q.PageSize > 100)
            q = q with { PageSize = 20 };

        var (items, total) = await _repo.GetPagedAsync(q.Gender, q.Page, q.PageSize);

        var dtos = items.Select(x => new VirtualPatientDto
        {
            PatientId = x.PatientId,
            ClinicalCaseId = x.ClinicalCaseId,
            Name = x.Name,
            Age = x.Age,
            Gender = x.Gender,
            Occupation = x.Occupation,
            Descriptions = x.Descriptions,
            ChiefConcern = x.ChiefConcern,
            
            VitalSigns = string.IsNullOrEmpty(x.VitalSigns) ? null : JsonSerializer.Deserialize<object>(x.VitalSigns),
            Instructions = string.IsNullOrEmpty(x.Instructions) ? null : JsonSerializer.Deserialize<object>(x.Instructions),
            CaseRules = string.IsNullOrEmpty(x.CaseRules) ? null : JsonSerializer.Deserialize<object>(x.CaseRules),
            Persona = string.IsNullOrEmpty(x.Persona) ? null : JsonSerializer.Deserialize<object>(x.Persona)
        }).ToList();

        return new PagedResult<VirtualPatientDto>
        {
            Items = dtos,
            Total = total,
            Page = q.Page,
            PageSize = q.PageSize
        };
    }
}