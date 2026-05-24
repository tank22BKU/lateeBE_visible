namespace VirtualPatientService.Application.Commands.FetchDiscoveryCases;

public record FetchDiscoveryCasesResponse(
    bool Success,
    string Message,
    FetchDiscoveryCasesData Data
);

public record FetchDiscoveryCasesData(
    string LearnerId,
    int FetchedCount,
    int CurrentPoolTotal,
    IReadOnlyList<FetchedPatientSummary> FetchedItems
);

public record FetchedPatientSummary(string PatientId, string CaseId, string Name, string Level);
