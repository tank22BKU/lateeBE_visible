using Microsoft.EntityFrameworkCore;
using AssessmentService.Domain.Entities;

namespace AssessmentService.Infrastructure.Persistance;

public class AssessmentDbContext : DbContext
{
    public AssessmentDbContext(DbContextOptions<AssessmentDbContext> options) : base(options) { }

    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<AssessmentQuestion> AssessmentQuestions => Set<AssessmentQuestion>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Assessment>(entity =>
        {
            entity.ToTable("assessments");
            entity.HasKey(x => x.AssessmentId);
            entity.Property(x => x.AssessmentId).HasColumnName("assessment_id");
            entity.Property(x => x.CreatorId).HasColumnName("creator_id");
            entity.Property(x => x.ClinicalCaseId).HasColumnName("clinical_case_id");
            entity.Property(x => x.CourseId).HasColumnName("course_id");
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
            entity.Property(x => x.GenerationPrompt).HasColumnName("generation_prompt");
            entity.Property(x => x.AllowedQuestionTypes).HasColumnName("allowed_question_types").HasColumnType("JSON");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
        });

        b.Entity<AssessmentQuestion>(entity =>
        {
            entity.ToTable("assessment_questions");
            entity.HasKey(x => x.QuestionId);
            entity.Property(x => x.QuestionId).HasColumnName("question_id");
            entity.Property(x => x.AssessmentId).HasColumnName("assessment_id");
            entity.Property(x => x.QuestionType).HasColumnName("question_type");
            entity.Property(x => x.CognitiveLevel).HasColumnName("cognitive_level");
            entity.Property(x => x.Content).HasColumnName("content");
            entity.Property(x => x.Options).HasColumnName("options").HasColumnType("JSON");
            entity.Property(x => x.Explanation).HasColumnName("explanation");
            entity.Property(x => x.Points).HasColumnName("points").HasColumnType("decimal(5,2)");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            
            entity.HasOne<Assessment>()
                    .WithMany(a => a.Questions)
                    .HasForeignKey(q => q.AssessmentId)
                    .OnDelete(DeleteBehavior.Cascade);
        });
    }
}