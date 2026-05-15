using Microsoft.EntityFrameworkCore;
using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repositories;
using AssessmentService.Infrastructure.Persistance;
namespace AssessmentService.Infrastructure.Repositories;

public class AssessmentRepository : IAssessmentRepository
{
    private readonly AssessmentDbContext _db;

    public AssessmentRepository(AssessmentDbContext db)
    {
        _db = db;
    }

    public async Task<Assessment?> GetByIdAsync(string id) => 
        await _db.Assessments.FirstOrDefaultAsync(a => a.AssessmentId == id);

    public async Task<Assessment?> GetByIdWithQuestionsAsync(string id) => 
        await _db.Assessments.Include(a => a.Questions).FirstOrDefaultAsync(a => a.AssessmentId == id);

    public async Task<(List<Assessment> Items, int Total)> GetPagedAsync(string? specialty, string? difficulty, int page, int pageSize)
    {
        var query = _db.Assessments.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(specialty)) query = query.Where(x => x.Specialty == specialty);
        if (!string.IsNullOrEmpty(difficulty)) query = query.Where(x => x.DifficultyLevel == difficulty);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.CreatedAt)
                               .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<List<Assessment>> GetAllAsync() => 
        await _db.Assessments.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync();

    public async Task AddAsync(Assessment assessment)
    {
        _db.Assessments.Add(assessment);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Assessment assessment)
    {
        _db.Assessments.Update(assessment);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Assessment assessment)
    {
        _db.Assessments.Remove(assessment);
        await _db.SaveChangesAsync();
    }

    public async Task AddQuestionsAsync(IEnumerable<Question> questions)
    {
        _db.Questions.AddRange(questions);
        await _db.SaveChangesAsync();
    }

    public async Task<Question?> GetQuestionByIdAsync(string questionId) => 
        await _db.Questions.FirstOrDefaultAsync(q => q.Id == questionId);

    public async Task AddQuestionAsync(Question question)
    {
        _db.Questions.Add(question);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateQuestionAsync(Question question)
    {
        _db.Questions.Update(question);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteQuestionAsync(Question question)
    {
        _db.Questions.Remove(question);
        await _db.SaveChangesAsync();
    }

    public async Task AddSessionAsync(AssessmentSession session)
    {
        _db.AssessmentSessions.Add(session);
        await _db.SaveChangesAsync();
    }

    public async Task<AssessmentSession?> GetSessionWithAnswersAsync(string sessionId)
    {
        return await _db.AssessmentSessions
            .Include(a => a.Answers) 
            .FirstOrDefaultAsync(a => a.SessionId == sessionId);
    }

    public async Task<List<AssessmentSession>> GetSessionsForLearnerAndAssessmentAsync(string learnerId, string assessmentId)
    {
        return await _db.AssessmentSessions
            .Include(s => s.Answers)
            .Where(s => s.LearnerId == learnerId && s.AssessmentId == assessmentId)
            .OrderByDescending(s => s.AttemptNo)
            .ToListAsync();
    }
}