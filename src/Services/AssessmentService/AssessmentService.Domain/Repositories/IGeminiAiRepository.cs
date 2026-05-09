namespace AssessmentService.Domain.Repositories;

public interface IGeminiAiRepository
{
    Task<string> GenerateQuestionsJsonAsync(string promptInstruction, int numQuestions);
    Task<string> GenerateQuestionsJsonAsyncVer2(string promptInstruction, int numQuestions, string language, string? pdfContent = null);

}