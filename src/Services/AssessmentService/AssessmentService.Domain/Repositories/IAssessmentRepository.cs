using AssessmentService.Domain.Entities;

namespace AssessmentService.Domain.Repositories;

public interface IAssessmentRepository
{
    Task<Assessment?> GetByIdAsync(string id);
    Task<Assessment?> GetByIdWithQuestionsAsync(string id);
    Task<(List<Assessment> Items, int Total)> GetPagedAsync(string? specialty, string? difficulty, int page, int pageSize);
    Task<List<Assessment>> GetAllAsync();

    Task AddAsync(Assessment assessment);
    Task UpdateAsync(Assessment assessment);
    Task DeleteAsync(Assessment assessment);
    Task AddQuestionsAsync(IEnumerable<Question> questions);

    Task<Question?> GetQuestionByIdAsync(string questionId);
    Task AddQuestionAsync(Question question);
    Task UpdateQuestionAsync(Question question);
    Task DeleteQuestionAsync(Question question);

    Task AddSessionAsync(AssessmentSession session);
    Task<AssessmentSession?> GetSessionWithAnswersAsync(string sessionId);
}