namespace AssessmentService.Application.Queries.GetAllAssessmentsOverviewOfLearner;

public class AssessmentDataDto
{
    public string AssessmentId { get; set; } = string.Empty; //
    public string CreatorId { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty; // 
    public string Topic { get; set; } = string.Empty; // 
    public string Subtopic { get; set; } = string.Empty; //
    public string DifficultyLevel { get; set; } = string.Empty; //
    public string Title { get; set; } = string.Empty; // 
    public string Descriptions { get; set; } = string.Empty; //
    public string Goal { get; set; } = string.Empty; //
    public int NumQuestions { get; set; } //
    public int TimeLimitMinutes { get; set; } // 
    public int TimesPracticed { get; set; } //
    public decimal MaxScore { get; set; } //
    public decimal PassingScorePercentage { get; set; }
    public int MaxAttempts { get; set; } //
    public bool IsActive { get; set; } //
    public DateTime CreatedAt { get; set; } //
    public List<AttemptItemDto> ListAttempts { get; set; } = new List<AttemptItemDto>();
}

public class AttemptItemDto
{
    public string AttempId { get; set; } = string.Empty;
    public string LearnerId { get; set; } = string.Empty;
    public int AttemptNo { get; set; }
    public int Duration { get; set; }
    public decimal Score { get; set; }
    public bool IsPassed { get; set; }
}