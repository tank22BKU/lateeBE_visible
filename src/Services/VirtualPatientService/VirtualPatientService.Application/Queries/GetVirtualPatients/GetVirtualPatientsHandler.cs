using System.Text.Json;
using MediatR;
using VirtualPatientService.Domain.Repositories;

namespace VirtualPatientService.Application.Queries.GetVirtualPatients;

public class GetVirtualPatientsHandler : IRequestHandler<GetVirtualPatientQuery, PagedResult<VirtualPatientDto>>
{
    private readonly IVirtualPatientRepository _repo;
    private readonly IClinicalCaseRepository _caseRepo;

    public GetVirtualPatientsHandler(IVirtualPatientRepository repo, IClinicalCaseRepository caseRepo)
    {
        _repo = repo;
        _caseRepo = caseRepo;
    }

    public async Task<PagedResult<VirtualPatientDto>> Handle(GetVirtualPatientQuery q, CancellationToken cancellationToken)
    {
        if (q.Page < 1) q = q with { Page = 1 };
        if (q.PageSize <= 0 || q.PageSize > 100)
            q = q with { PageSize = 20 };

        var (items, total) = await _repo.GetPagedAsync(q.Gender, q.Page, q.PageSize);

        var caseMap = await _caseRepo.GetByIdsAsync(items.Select(x => x.CaseId));

        var dtos = items.Select(x =>
        {
            caseMap.TryGetValue(x.CaseId, out var clinicalCase);

            return new VirtualPatientDto
            {
            PatientId = x.PatientId,
            CaseId = x.CaseId,
            Name = x.Name,
            Age = x.Age,
            Gender = x.Gender,
            Occupation = x.Occupation,
            ChiefConcern = x.ChiefConcern,
            MedicalHistory = clinicalCase?.MedicalHistory,
            Symptom = clinicalCase?.Symptom,
            Pronouns = x.Pronouns,
            Ethnicity = x.Ethnicity,
            Persona = ParseJsonOrString(x.Persona),
            VitalSigns = ParseJsonOrString(x.VitalSigns),
            Instructions = ParseJsonOrString(x.Instructions),
            Behaviors = ParseJsonOrString(x.Behaviors),
            TimeSetting = x.TimeSetting,
            ArgumentTime = x.ArgumentTime,
            LearningObjectives = ParseJsonOrString(x.LearningObjectives),
            Level = x.Level,
            AvatarImage = x.AvatarImage,
            CaseRule = ParseJsonOrString(x.CaseRule),
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
            };
        }).ToList();

        return new PagedResult<VirtualPatientDto>
        {
            Items = dtos,
            Total = total,
            Page = q.Page,
            PageSize = q.PageSize
        };
    }

    private static object? ParseJsonOrString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        try
        {
            return JsonSerializer.Deserialize<object>(value);
        }
        catch
        {
            return value;
        }
    }
}