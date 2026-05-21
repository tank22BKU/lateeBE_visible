using MediatR;
using VirtualPatientService.Application.Queries.GetVirtualPatientDiscovery;

namespace VirtualPatientService.Application.Queries.GetVirtualPatientDiscovery;

public class GetVirtualPatientDiscoveryQuery : IRequest<GetVirtualPatientDiscoveryResponse>
{
    public string LearnerId { get; set; } = default!;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 9;
    public string? Level { get; set; }
    public string? Occupation { get; set; }
    public string? ExpertId { get; set; }
    public string? Gender { get; set; }
    public string? Specialty { get; set; }
    public string? CaseType { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
}