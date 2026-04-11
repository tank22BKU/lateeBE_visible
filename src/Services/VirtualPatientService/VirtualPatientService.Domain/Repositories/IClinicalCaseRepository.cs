using VirtualPatientService.Domain.Entities;

namespace VirtualPatientService.Domain.Repositories;

public interface IVirtualPatientRepository
{
    Task<VirtualPatient?> GetByIdAsync(string patientId);
    Task<List<VirtualPatient>> GetAllAsync();
    Task<(List<VirtualPatient> Items, int Total)> GetPagedAsync(string? gender, int page, int pageSize);
}
