using System.Text.Json;

namespace VirtualPatientService.Application.Dtos;

public class VirtualPatientExpertListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Level { get; set; }
    public string? Gender { get; set; }
    public string? CaseId { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
}

public class VirtualPatientExpertUpsertRequest
{
    public string? PatientId { get; set; }
    public string CaseId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Pronouns { get; set; }
    public string? Ethnicity { get; set; }
    public string? Occupation { get; set; }
    public string? ChiefConcern { get; set; }
    public JsonElement? Persona { get; set; }
    public JsonElement? VitalSigns { get; set; }
    public JsonElement? Instructions { get; set; }
    public JsonElement? Behaviors { get; set; }
    public int? TimeSetting { get; set; }
    public int? ArgumentTime { get; set; }
    public JsonElement? LearningObjectives { get; set; }
    public string? Level { get; set; }
    public string? AvatarImage { get; set; }
    public JsonElement? CaseRule { get; set; }
    public List<string> ExpertIds { get; set; } = new();
}

public class VirtualPatientExpertStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class VirtualPatientExpertPublishRequest
{
    public bool Publish { get; set; }
}

public class VirtualPatientExpertListResponseDto
{
    public List<VirtualPatientExpertListItemDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public VirtualPatientExpertFiltersDto Filters { get; set; } = new();
}

public class VirtualPatientExpertListItemDto
{
    public string PatientId { get; set; } = string.Empty;
    public string CaseId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Occupation { get; set; }
    public string? ChiefConcern { get; set; }
    public string? Level { get; set; }
    public string? Status { get; set; }
    public string? AvatarImage { get; set; }
    public int? TimeSetting { get; set; }
    public int? ArgumentTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int AttemptCount { get; set; }
    public decimal? AvgScore { get; set; }
    public int ExpertCount { get; set; }
}

public class VirtualPatientExpertDetailDto
{
    public string PatientId { get; set; } = string.Empty;
    public string CaseId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Pronouns { get; set; }
    public string? Ethnicity { get; set; }
    public string? Occupation { get; set; }
    public string? ChiefConcern { get; set; }
    public string? MedicalHistory { get; set; }
    public string? Symptom { get; set; }
    public object? Persona { get; set; }
    public object? VitalSigns { get; set; }
    public object? Instructions { get; set; }
    public object? Behaviors { get; set; }
    public int? TimeSetting { get; set; }
    public int? ArgumentTime { get; set; }
    public object? LearningObjectives { get; set; }
    public string? Level { get; set; }
    public string? AvatarImage { get; set; }
    public object? CaseRule { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ExpertDto> Experts { get; set; } = new();
    public VirtualPatientExpertStatsDto Stats { get; set; } = new();
}

public class VirtualPatientExpertStatsDto
{
    public int TotalAttempts { get; set; }
    public decimal? AvgScore { get; set; }
    public decimal CompletionRate { get; set; }
}

public class VirtualPatientExpertFiltersDto
{
    public List<string> AvailableStatuses { get; set; } = new();
    public List<string> AvailableLevels { get; set; } = new();
    public List<string> AvailableGenders { get; set; } = new();
    public List<string> AvailableCaseIds { get; set; } = new();
}
