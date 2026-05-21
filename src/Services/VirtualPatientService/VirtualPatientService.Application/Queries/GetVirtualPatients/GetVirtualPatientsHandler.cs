using System.Text.Json;
using MediatR;
using VirtualPatientService.Application.Dtos;
using VirtualPatientService.Domain.Constants;
using VirtualPatientService.Domain.Repositories;

namespace VirtualPatientService.Application.Queries.GetVirtualPatients;

public class GetVirtualPatientsHandler
    : IRequestHandler<GetVirtualPatientsQuery, PageResult<VirtualPatientListItemDto>>
{
    private readonly IVirtualPatientRepository _vpRepo;
    private readonly IClinicalCaseRepository _caseRepo;

    public GetVirtualPatientsHandler(
        IVirtualPatientRepository vpRepo,
        IClinicalCaseRepository caseRepo
    )
    {
        _vpRepo = vpRepo;
        _caseRepo = caseRepo;
    }

    public async Task<PageResult<VirtualPatientListItemDto>> Handle(
        GetVirtualPatientsQuery request,
        CancellationToken cancellationToken
    )
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize =
            request.PageSize <= 0 || request.PageSize > VirtualPatientConstants.MaxPageSize
                ? VirtualPatientConstants.DefaultPageSize
                : request.PageSize;

        var (items, total) = await _vpRepo.GetPagedAsync(
            request.Gender,
            page,
            pageSize,
            cancellationToken
        );

        var caseMap = await _caseRepo.GetByIdsAsync(items.Select(x => x.CaseId), cancellationToken);
        var expertsByPatient = await _vpRepo.GetExpertsByPatientIdsAsync(
            items.Select(x => x.PatientId),
            cancellationToken
        );

        var dtos = items
            .Select(x =>
            {
                caseMap.TryGetValue(x.CaseId, out var clinicalCase);
                expertsByPatient.TryGetValue(x.PatientId, out var experts);
                return new VirtualPatientListItemDto
                {
                    PatientId = x.PatientId,
                    CaseId = x.CaseId,
                    Name = x.Name,
                    Age = x.Age,
                    Gender = x.Gender,
                    Pronouns = x.Pronouns,
                    Ethnicity = x.Ethnicity,
                    Occupation = x.Occupation,
                    ChiefConcern = x.ChiefConcern,
                    MedicalHistory = clinicalCase?.MedicalHistory,
                    Symptom = clinicalCase?.Symptom,
                    Persona = ParseJson(x.Persona),
                    VitalSigns = ParseJson(x.VitalSigns),
                    Instructions = ParseJson(x.Instructions),
                    Behaviors = ParseJson(x.Behaviors),
                    TimeSetting = x.TimeSetting,
                    ArgumentTime = x.ArgumentTime,
                    LearningObjectives = ParseJson(x.LearningObjectives),
                    Level = x.Level,
                    AvatarImage = x.AvatarImage,
                    CaseRule = ParseJson(x.CaseRule),
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Experts = (experts ?? [])
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
            })
            .ToList();

        return new PageResult<VirtualPatientListItemDto>
        {
            Items = dtos,
            Total = total,
            Page = page,
            PageSize = pageSize,
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
