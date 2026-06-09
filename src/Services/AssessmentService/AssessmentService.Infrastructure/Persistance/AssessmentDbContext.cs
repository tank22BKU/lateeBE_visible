using Microsoft.EntityFrameworkCore;
using AssessmentService.Domain.Entities;

namespace AssessmentService.Infrastructure.Persistance;

public class AssessmentDbContext : DbContext
{
    public AssessmentDbContext(DbContextOptions<AssessmentDbContext> options) : base(options) { }

    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AssessmentSession> AssessmentSessions => Set<AssessmentSession>();
    public DbSet<AssessmentAnswer> AssessmentAnswers => Set<AssessmentAnswer>();
    public DbSet<Users> Users => Set<Users>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Assessment>(entity =>
        {
            entity.ToTable("assessments");
            entity.HasKey(x => x.AssessmentId);
            entity.Property(x => x.AssessmentId).HasColumnName("assessment_id");
            entity.Property(x => x.ModuleId).HasColumnName("module_id");
            entity.Property(x => x.Specialty).HasColumnName("specialty");
            entity.Property(x => x.Topic).HasColumnName("topic");
            entity.Property(x => x.Subtopic).HasColumnName("subtopic");
            entity.Property(x => x.DifficultyLevel).HasColumnName("difficulty_level");
            entity.Property(x => x.Title).HasColumnName("title");
            entity.Property(x => x.Descriptions).HasColumnName("descriptions");
            entity.Property(x => x.Goal).HasColumnName("goal");
            entity.Property(x => x.NumQuestions).HasColumnName("num_questions");
            entity.Property(x => x.TimeLimitMinutes).HasColumnName("time_limit_minutes");
            entity.Property(x => x.PassingScorePercentage).HasColumnName("passing_score_percentage").HasColumnType("decimal(5,2)");
            entity.Property(x => x.MaxAttempts).HasColumnName("max_attempts");
            entity.Property(x => x.AllowedQuestionTypes).HasColumnName("allowed_question_types").HasColumnType("JSON");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
        });

        b.Entity<Question>(entity =>
        {
            entity.ToTable("question");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.AssessmentId).HasColumnName("assessment_id");
            entity.Property(x => x.Content).HasColumnName("question");
            entity.Property(x => x.QuestionOption).HasColumnName("question_option").HasColumnType("JSON");
            entity.Property(x => x.QuestionType).HasColumnName("question_type");
            entity.Property(x => x.CognitiveLevel).HasColumnName("cognitive_level");
            entity.Property(x => x.Explanation).HasColumnName("explanation");
            entity.Property(x => x.Points).HasColumnName("points").HasColumnType("decimal(5,2)");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<Assessment>()
                .WithMany(a => a.Questions)
                .HasForeignKey(q => q.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        b.Entity<Users>(entity =>
        {
            entity.HasKey(x => x.UserId);
            entity.ToTable("users");      
        });


        b.Entity<AssessmentSession>(entity =>
        {
            entity.ToTable("assessment_session");
            entity.HasKey(x => x.SessionId);
            entity.Property(x => x.SessionId).HasColumnName("session_id");
            entity.Property(x => x.AssessmentId).HasColumnName("assessment_id");
            entity.Property(x => x.OverallScore).HasColumnName("overall_score").HasColumnType("decimal(5,2)");
            entity.Property(x => x.LearnerId).HasColumnName("learner_id");
            entity.Property(x => x.AttemptNo).HasColumnName("attempt_no");
            entity.Property(x => x.Duration).HasColumnName("duration");
            entity.Property(x => x.StartTime).HasColumnName("start_time");
            entity.Property(x => x.EndTime).HasColumnName("end_time");
            entity.Property(x => x.Status).HasColumnName("status");
            entity.Property(x => x.IsPassed).HasColumnName("is_passed");
        });

        b.Entity<AssessmentAnswer>(entity =>
        {
            entity.ToTable("assessment_answer");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SessionId).HasColumnName("session_id");
            entity.Property(x => x.QuestionId).HasColumnName("question_id");
            entity.Property(x => x.UserChoice).HasColumnName("user_choice").HasColumnType("JSON");
            entity.Property(x => x.IsCorrect).HasColumnName("is_correct");
            entity.Property(x => x.PointsEarned).HasColumnName("points_earned").HasColumnType("decimal(5,2)");
            entity.Property(x => x.IsFlagged).HasColumnName("is_flagged");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasOne<AssessmentSession>()
                .WithMany(a => a.Answers)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        }
}