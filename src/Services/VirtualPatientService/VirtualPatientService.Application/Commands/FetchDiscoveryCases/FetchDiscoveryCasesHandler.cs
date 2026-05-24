using MediatR;
using VirtualPatientService.Domain.Entities;
using VirtualPatientService.Domain.Repositories;

namespace VirtualPatientService.Application.Commands.FetchDiscoveryCases;

public class FetchDiscoveryCasesHandler
    : IRequestHandler<FetchDiscoveryCasesCommand, FetchDiscoveryCasesResponse>
{
    private readonly ILearnerDiscoveryPoolRepository _poolRepo;

    public FetchDiscoveryCasesHandler(ILearnerDiscoveryPoolRepository poolRepo)
    {
        _poolRepo = poolRepo;
    }

    public async Task<FetchDiscoveryCasesResponse> Handle(
        FetchDiscoveryCasesCommand request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.LearnerId))
            throw new LearnerNotFoundException(
                "The provided learnerId does not exist or session has expired."
            );

        if (request.FetchCount < 1 || request.FetchCount > 20)
            throw new FetchCasesValidationException(
                "INVALID_FETCH_COUNT",
                "The fetchCount parameter must be an integer between 1 and 20."
            );

        var normalizedLevel = NormalizeLevel(request.Level);
        var normalizedGender = NormalizeGender(request.Gender);

        if (!string.IsNullOrWhiteSpace(request.Level) && normalizedLevel is null)
        {
            throw new FetchCasesValidationException(
                "INVALID_LEVEL_FILTER",
                "The level parameter must be one of: Beginner, Intermediate, Advanced, Expert."
            );
        }

        if (!string.IsNullOrWhiteSpace(request.Gender) && normalizedGender is null)
        {
            throw new FetchCasesValidationException(
                "INVALID_GENDER_FILTER",
                "The gender parameter must be either MALE or FEMALE."
            );
        }

        var selectedCases = await _poolRepo.GetRandomAvailableCasesAsync(
            request.LearnerId,
            normalizedLevel,
            normalizedGender,
            request.FetchCount,
            cancellationToken
        );

        if (selectedCases.Count == 0)
            throw new NoMoreCasesAvailableException(
                "No new patient cases match your criteria. Try changing filters."
            );

        var poolEntries = selectedCases.Select(x => new LearnerDiscoveryPool
        {
            Id = Guid.NewGuid().ToString(),
            LearnerId = request.LearnerId,
            PatientId = x.PatientId,
            FetchedAt = DateTime.UtcNow,
            FetchLevel = normalizedLevel,
            FetchGender = normalizedGender,
        });

        await _poolRepo.AddRangeAsync(poolEntries, cancellationToken);

        var currentPoolTotal = await _poolRepo.GetPoolTotalAsync(
            request.LearnerId,
            cancellationToken
        );
        var fetchedCount = selectedCases.Count;

        var message =
            fetchedCount == request.FetchCount
                ? $"Successfully fetched {fetchedCount} new virtual patient cases."
                : $"Only {fetchedCount} new cases were available matching your criteria.";

        return new FetchDiscoveryCasesResponse(
            Success: true,
            Message: message,
            Data: new FetchDiscoveryCasesData(
                LearnerId: request.LearnerId,
                FetchedCount: fetchedCount,
                CurrentPoolTotal: currentPoolTotal,
                FetchedItems: selectedCases
                    .Select(x => new FetchedPatientSummary(
                        x.PatientId,
                        x.CaseId,
                        x.Name,
                        x.Level ?? string.Empty
                    ))
                    .ToList()
            )
        );
    }

    private static string? NormalizeLevel(string? level) =>
        level?.Trim() switch
        {
            null or "" => null,
            "Beginner" or "beginner" => "Beginner",
            "Intermediate" or "intermediate" => "Intermediate",
            "Advanced" or "advanced" => "Advanced",
            "Expert" or "expert" => "Expert",
            _ => null,
        };

    private static string? NormalizeGender(string? gender) =>
        gender?.Trim() switch
        {
            null or "" => null,
            "MALE" or "male" => "MALE",
            "FEMALE" or "female" => "FEMALE",
            _ => null,
        };
}
