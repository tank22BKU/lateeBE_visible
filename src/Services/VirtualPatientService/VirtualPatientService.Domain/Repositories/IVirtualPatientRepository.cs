using VirtualPatientService.Domain.Entities;

namespace VirtualPatientService.Domain.Repositories;

public interface IVirtualPatientRepository
{
	Task<VirtualPatient?> GetByIdAsync(string patientId);
	Task<List<VirtualPatient>> GetAllAsync();
	Task<(List<VirtualPatient> Items, int Total)> GetPagedAsync(string? gender, int page, int pageSize);
	Task<List<ExpertSummary>> GetExpertsByPatientIdAsync(string patientId);
}

public record ExpertSummary(
	string ExpertId,
	string Name,
	string? Role,
	string? AvatarUrl,
	string? BioQuote,
	string? EducationDetail,
	string? ExpertiseSkill,
	string? Phone,
	string? Email,
	string? Location
);