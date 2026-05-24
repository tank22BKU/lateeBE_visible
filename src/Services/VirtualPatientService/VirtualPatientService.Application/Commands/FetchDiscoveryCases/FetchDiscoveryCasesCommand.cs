using MediatR;

namespace VirtualPatientService.Application.Commands.FetchDiscoveryCases;

public record FetchDiscoveryCasesCommand(
    string LearnerId,
    string? Level,
    string? Gender,
    int FetchCount
) : IRequest<FetchDiscoveryCasesResponse>;
