using MediatR;
using System.Text.Json;
using VirtualPatientService.Application.Queries.GetVirtualPatients;
using VirtualPatientService.Domain.Repositories;

namespace VirtualPatientService.Application.Queries.GetVirtualPatientByID;

public class GetVirtualPatientByIdHandler : IRequestHandler<GetVirtualPatientByIdQuery, VirtualPatientDto?>
{
    private readonly IVirtualPatientRepository _repo;
    private readonly IClinicalCaseRepository _caseRepo;

    public GetVirtualPatientByIdHandler(IVirtualPatientRepository repo, IClinicalCaseRepository caseRepo)
    {
        _repo = repo;
        _caseRepo = caseRepo;
    }

    public async Task<VirtualPatientDto?> Handle(GetVirtualPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var x = await _repo.GetByIdAsync(request.PatientId);
        
        if (x == null) return null;

        var clinicalCase = await _caseRepo.GetByIdAsync(x.CaseId);

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