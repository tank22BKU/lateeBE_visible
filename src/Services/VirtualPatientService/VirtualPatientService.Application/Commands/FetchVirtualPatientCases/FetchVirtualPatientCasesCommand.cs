using MediatR;

namespace VirtualPatientService.Application.Commands.FetchVirtualPatientCases;

public class FetchVirtualPatientCasesCommand : IRequest<FetchVirtualPatientCasesResponse>
{
    public string LearnerId { get; set; } = default!;
    public string? Level { get; set; }
    public string? Gender { get; set; }
    public int FetchCount { get; set; }
}
