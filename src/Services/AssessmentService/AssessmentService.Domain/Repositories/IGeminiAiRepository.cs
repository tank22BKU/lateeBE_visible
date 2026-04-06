namespace AssessmentService.Domain.Repositories;

public interface IGeminiAiRepository
{
    Task<string> GenerateQuestionsJsonAsync(string promptInstruction, int numQuestions);
}