namespace AssessmentService.Domain.Entities;

public class AssessmentQuestion
{
    public string QuestionId { get; set; } = Guid.NewGuid().ToString("N");
    public string AssessmentId { get; set; } = null!;
    public string QuestionType { get; set; } = "MultipleChoice";
    public string? CognitiveLevel { get; set; } 
    public string Content { get; set; } = null!;
    public string? Options { get; set; } 
    public string? Explanation { get; set; }
    public decimal Points { get; set; } = 1.00m;
    public DateTime CreatedAt { get; set; }
}