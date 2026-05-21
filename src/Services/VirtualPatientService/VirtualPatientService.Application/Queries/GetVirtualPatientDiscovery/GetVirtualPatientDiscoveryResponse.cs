using VirtualPatientService.Application.Dtos;

namespace VirtualPatientService.Application.Queries.GetVirtualPatientDiscovery;

public class GetVirtualPatientDiscoveryResponse
{
    public List<VirtualPatientDiscoveryItemDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public DiscoveryFiltersDto Filters { get; set; } = new();
}