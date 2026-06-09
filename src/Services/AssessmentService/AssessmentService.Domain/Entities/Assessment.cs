namespace AssessmentService.Domain.Entities;

public class Assessment
{
    public string AssessmentId { get; set; } = Guid.NewGuid().ToString("N");
    public string? ModuleId { get; set; }
    public string? Specialty { get; set; }
    public string Topic { get; set; } = null!;
    public string? Subtopic { get; set; }
    public string DifficultyLevel { get; set; } = "Intermediate";
    public string Title { get; set; } = null!;
    public string? Descriptions { get; set; }
    public string? Goal { get; set; }
    public int NumQuestions { get; set; } = 10;
    public int? TimeLimitMinutes { get; set; }
    public decimal PassingScorePercentage { get; set; } = 80.00m;
    public int MaxAttempts { get; set; } = 1;
    public string? AllowedQuestionTypes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}