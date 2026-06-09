namespace AssessmentService.Application.DTOs;

public class GeneratedQuestionDto
{
    public string QuestionType { get; set; } = string.Empty;
    public string CognitiveLevel { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public object Options { get; set; } = null!; 
    public string Explanation { get; set; } = string.Empty;
}