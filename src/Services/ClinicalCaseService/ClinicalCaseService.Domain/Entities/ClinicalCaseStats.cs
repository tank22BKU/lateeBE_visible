namespace ClinicalCaseService.Domain.Entities;

public class ClinicalCaseStats
{
    [System.Text.Json.Serialization.JsonIgnore]
    public int VirtualPatientCount { get; set; }

    public int TotalAttempts { get; set; }
    public decimal AvgScore { get; set; }
    public decimal CompletionRate { get; set; }
}
