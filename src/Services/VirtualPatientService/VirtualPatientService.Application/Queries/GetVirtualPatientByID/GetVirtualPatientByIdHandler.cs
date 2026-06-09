using System.Text.Json;
using MediatR;
using VirtualPatientService.Application.Dtos;
using VirtualPatientService.Domain.Repositories;

namespace VirtualPatientService.Application.Queries.GetVirtualPatientById;

public class GetVirtualPatientByIdHandler
    : IRequestHandler<GetVirtualPatientByIdQuery, VirtualPatientDetailDto?>
{
    private readonly IVirtualPatientRepository _vpRepo;
    private readonly IClinicalCaseRepository _caseRepo;

    public GetVirtualPatientByIdHandler(
        IVirtualPatientRepository vpRepo,
        IClinicalCaseRepository caseRepo
    )
    {
        _vpRepo = vpRepo;
        _caseRepo = caseRepo;
    }

    public async Task<VirtualPatientDetailDto?> Handle(
        GetVirtualPatientByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var patient = await _vpRepo.GetByIdAsync(request.PatientId, cancellationToken);
        if (patient is null)
            return null;

        var clinicalCase = await _caseRepo.GetByIdAsync(patient.CaseId, cancellationToken);

        var experts = await _vpRepo.GetExpertsByPatientIdAsync(
            patient.PatientId,
            cancellationToken
        );

        return new VirtualPatientDetailDto
        {
            PatientId = patient.PatientId,
            CaseId = patient.CaseId,
            Name = patient.Name,
            Age = patient.Age,
            Gender = patient.Gender,
            Pronouns = patient.Pronouns,
            Ethnicity = patient.Ethnicity,
            Occupation = patient.Occupation,
            ChiefConcern = patient.ChiefConcern,
            MedicalHistory = clinicalCase?.MedicalHistory,
            Symptom = clinicalCase?.Symptom,
            Persona = ParseJson(patient.Persona),
            VitalSigns = ParseJson(patient.VitalSigns),
            Instructions = ParseJson(patient.Instructions),
            Behaviors = ParseJson(patient.Behaviors),
            TimeSetting = patient.TimeSetting,
            ArgumentTime = patient.ArgumentTime,
            LearningObjectives = ParseJson(patient.LearningObjectives),
            Level = patient.Level,
            AvatarImage = patient.AvatarImage,
            CaseRule = ParseJson(patient.CaseRule),
            Status = patient.Status,
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt,
            Experts = experts
                .Select(e => new ExpertDto
                {
                    ExpertId = e.ExpertId,
                    Name = e.Name,
                    Role = e.Role,
                    AvatarUrl = e.AvatarUrl,
                    BioQuote = e.BioQuote,
                    EducationDetail = e.EducationDetail,
                    ExpertiseSkill = e.ExpertiseSkill,
                    Phone = e.Phone,
                    Email = e.Email,
                    Location = null, 
                })
                .ToList(),
        };
    }

    private static object? ParseJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
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
