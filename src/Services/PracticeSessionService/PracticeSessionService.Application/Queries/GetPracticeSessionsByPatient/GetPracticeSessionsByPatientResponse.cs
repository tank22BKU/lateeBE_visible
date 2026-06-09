using System.Text.Json.Serialization;

namespace PracticeSessionService.Application.Queries.GetPracticeSessionsByPatient;

public class GetPracticeSessionsByPatientResponse
{
    public string LearnerId { get; set; } = default!;
    public string PatientId { get; set; } = default!;
    public List<PracticeSessionItemResponse> Items { get; set; } = [];
}

public class PracticeSessionItemResponse
{
    public string SessionId { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime CreatedAt { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FinalDiagnosis { get; set; }
}
