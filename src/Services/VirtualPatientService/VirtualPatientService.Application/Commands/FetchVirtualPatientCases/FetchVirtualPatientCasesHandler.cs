using MediatR;
using VirtualPatientService.Domain.Repositories;

namespace VirtualPatientService.Application.Commands.FetchVirtualPatientCases;

public class FetchVirtualPatientCasesHandler
    : IRequestHandler<FetchVirtualPatientCasesCommand, FetchVirtualPatientCasesResponse>
{
    private readonly IVirtualPatientFetchRepository _fetchRepo;

    public FetchVirtualPatientCasesHandler(IVirtualPatientFetchRepository fetchRepo)
    {
        _fetchRepo = fetchRepo;
    }

    public async Task<FetchVirtualPatientCasesResponse> Handle(
        FetchVirtualPatientCasesCommand request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.LearnerId))
            throw new LearnerNotFoundException(
                "The provided learnerId does not exist or session has expired."
            );

        if (request.FetchCount < 1 || request.FetchCount > 20)
        {
            throw new FetchCasesValidationException(
                "INVALID_FETCH_COUNT",
                "The fetchCount parameter must be an integer between 1 and 20."
            );
        }

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

        var learnerExists = await _fetchRepo.LearnerExistsAsync(
            request.LearnerId,
            cancellationToken
        );
        if (!learnerExists)
        {
            throw new LearnerNotFoundException(
                "The provided learnerId does not exist or session has expired."
            );
        }

        var ownedPatientIds = await _fetchRepo.GetOwnedPatientIdsAsync(
            request.LearnerId,
            cancellationToken
        );

        var availableCases = await _fetchRepo.GetAvailableCasesAsync(
            normalizedLevel,
            normalizedGender,
            ownedPatientIds,
            cancellationToken
        );

        if (availableCases.Count == 0)
        {
            throw new NoMoreCasesAvailableException(
                "No new patient cases match your criteria in the system database. Try changing the difficulty level or gender filters."
            );
        }

        var selectedCases = availableCases
            .OrderBy(_ => Random.Shared.Next())
            .Take(Math.Min(request.FetchCount, availableCases.Count))
            .ToList();

        await _fetchRepo.SaveFetchedCasesAsync(
            request.LearnerId,
            selectedCases.Select(x => x.PatientId),
            cancellationToken
        );

        var fetchedCount = selectedCases.Count;
        var currentPoolTotal = ownedPatientIds.Count + fetchedCount;

        return new FetchVirtualPatientCasesResponse
        {
            Success = true,
            Message =
                $"Successfully fetched {fetchedCount} new virtual patient cases from the system database.",
            Data = new FetchVirtualPatientCasesData
            {
                LearnerId = request.LearnerId,
                FetchedCount = fetchedCount,
                CurrentPoolTotal = currentPoolTotal,
                FetchedItems = selectedCases
                    .Select(x => new FetchedVirtualPatientCaseItemDto
                    {
                        PatientId = x.PatientId,
                        CaseId = x.CaseId,
                        Name = x.Name,
                        Level = x.Level,
                    })
                    .ToList(),
            },
        };
    }

    private static string? NormalizeLevel(string? level) =>
        level?.Trim() switch
        {
            null or "" => null,
            "Beginner" => "Beginner",
            "beginner" => "Beginner",
            "Intermediate" => "Intermediate",
            "intermediate" => "Intermediate",
            "Advanced" => "Advanced",
            "advanced" => "Advanced",
            "Expert" => "Expert",
            "expert" => "Expert",
            _ => null,
        };

    private static string? NormalizeGender(string? gender) =>
        gender?.Trim() switch
        {
            null or "" => null,
            "MALE" => "MALE",
            "male" => "MALE",
            "FEMALE" => "FEMALE",
            "female" => "FEMALE",
            _ => null,
        };
}
