namespace ClinicalCaseService.Domain.Entities;

public class ClinicalCaseRadiology
{
    public int Id { get; set; }
    public string? NoteId { get; set; }
    public string? Modality { get; set; }
    public string? Region { get; set; }
    public string? ExamName { get; set; }
    public string? Text { get; set; }
}
