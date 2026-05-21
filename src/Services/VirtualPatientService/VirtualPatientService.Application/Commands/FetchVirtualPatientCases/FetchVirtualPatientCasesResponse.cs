namespace VirtualPatientService.Application.Commands.FetchVirtualPatientCases;

public class FetchVirtualPatientCasesResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public FetchVirtualPatientCasesData Data { get; set; } = new();
}

public class FetchVirtualPatientCasesData
{
    public string LearnerId { get; set; } = default!;
    public int FetchedCount { get; set; }
    public int CurrentPoolTotal { get; set; }
    public List<FetchedVirtualPatientCaseItemDto> FetchedItems { get; set; } = new();
}

public class FetchedVirtualPatientCaseItemDto
{
    public string PatientId { get; set; } = default!;
    public string CaseId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Level { get; set; }
}
