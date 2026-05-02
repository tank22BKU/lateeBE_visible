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
    Task AddQuestionsAsync(IEnumerable<AssessmentQuestion> questions);

    Task<AssessmentQuestion?> GetQuestionByIdAsync(string questionId);
    Task AddQuestionAsync(AssessmentQuestion question);
    Task UpdateQuestionAsync(AssessmentQuestion question);
    Task DeleteQuestionAsync(AssessmentQuestion question);

    Task AddAttemptAsync(AssessmentAttempt attempt);
    Task<AssessmentAttempt?> GetAttemptWithAnswersAsync(string attemptId);
}