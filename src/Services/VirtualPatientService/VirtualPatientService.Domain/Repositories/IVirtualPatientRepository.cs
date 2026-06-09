using VirtualPatientService.Domain.Entities;

namespace VirtualPatientService.Domain.Repositories;

public interface IVirtualPatientRepository
{
    Task<VirtualPatient?> GetByIdAsync(
        string patientId,
        CancellationToken cancellationToken = default
    );

    Task<(List<VirtualPatient> Items, int Total)> GetPagedAsync(
        string? gender,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<(List<VirtualPatientProjection> Items, int Total)> GetPagedForDiscoveryAsync(
        int page,
        int pageSize,
        string? level,
        string? occupation,
        string? expertId,
        string? gender,
        string? specialty,
        string? caseType,
        string? search,
        string sortBy,
        CancellationToken cancellationToken = default
    );

    Task<List<ExpertWithUserProjection>> GetExpertsByPatientIdAsync(
        string patientId,
        CancellationToken cancellationToken = default
    );

    Task<Dictionary<string, List<ExpertWithUserProjection>>> GetExpertsByPatientIdsAsync(
        IEnumerable<string> patientIds,
        CancellationToken cancellationToken = default
    );

    Task<DiscoveryFiltersProjection> GetDiscoveryFiltersAsync(
        CancellationToken cancellationToken = default
    );
}

public record VirtualPatientProjection(
    string PatientId,
    string CaseId,
    string Name,
    int? Age,
    string? Gender,
    string? Occupation,
    string? ChiefConcern,
    string? Symptom,
    string? Level,
    string? AvatarImage,
    int? TimeSetting,
    int? ArgumentTime,
    DateTime CreatedAt
);

public record ExpertWithUserProjection(
    string ExpertId,
    string VirtualId,
    string? Name,
    string? Role,
    string? AvatarUrl,
    string? BioQuote,
    string? EducationDetail,
    string? ExpertiseSkill,
    string? Phone,
    string? Email
);

public record DiscoveryFiltersProjection(
    List<string> AvailableLevels,
    List<string> AvailableGenders,
    List<string> AvailableSpecialties,
    List<string> AvailableCaseTypes
);
