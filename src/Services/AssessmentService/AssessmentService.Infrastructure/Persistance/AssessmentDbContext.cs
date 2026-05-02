using Microsoft.EntityFrameworkCore;
using AssessmentService.Domain.Entities;

namespace AssessmentService.Infrastructure.Persistance;

public class AssessmentDbContext : DbContext
{
    public AssessmentDbContext(DbContextOptions<AssessmentDbContext> options) : base(options) { }

    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<AssessmentQuestion> AssessmentQuestions => Set<AssessmentQuestion>();

    public DbSet<AssessmentAttempt> AssessmentAttempts => Set<AssessmentAttempt>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();
    public DbSet<Users> Users => Set<Users>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Assessment>(entity =>
        {
            entity.ToTable("assessments");
            entity.HasKey(x => x.AssessmentId);
            entity.Property(x => x.AssessmentId).HasColumnName("assessment_id");
            entity.Property(x => x.CreatorId).HasColumnName("creator_id").IsRequired(true);
            // entity.HasIndex(x => x.CreatorId).HasDatabaseName("fk_assessment_creator");
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
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            // entity.HasOne<Users>() 
            //     .WithMany()
            //     .HasForeignKey(x => x.CreatorId)
            //     .OnDelete(DeleteBehavior.Cascade);

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


        b.Entity<Users>(entity =>
        {
            entity.HasKey(x => x.UserId);
            entity.ToTable("users");      
        });


        b.Entity<AssessmentAttempt>(entity =>
        {
            entity.ToTable("assessment_attempts");
            entity.HasKey(x => x.AttemptId);
            entity.Property(x => x.AttemptId).HasColumnName("attempt_id");
            entity.Property(x => x.AssessmentId).HasColumnName("assessment_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.StartTime).HasColumnName("start_time");
            entity.Property(x => x.EndTime).HasColumnName("end_time");
            entity.Property(x => x.Score).HasColumnName("score").HasColumnType("decimal(5,2)");
            entity.Property(x => x.IsPassed).HasColumnName("is_passed");
            entity.Property(x => x.Status).HasColumnName("status");
        });

        b.Entity<AttemptAnswer>(entity =>
        {
            entity.ToTable("attempt_answers");
            entity.HasKey(x => x.AnswerId);
            entity.Property(x => x.AnswerId).HasColumnName("answer_id");
            entity.Property(x => x.AttemptId).HasColumnName("attempt_id");
            entity.Property(x => x.QuestionId).HasColumnName("question_id");
            entity.Property(x => x.UserChoice).HasColumnName("user_choice")
                .HasColumnType("json") 
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<string>(v, (System.Text.Json.JsonSerializerOptions)null)
                );
            entity.Property(x => x.IsCorrect).HasColumnName("is_correct");
            entity.Property(x => x.PointsEarned).HasColumnName("points_earned").HasColumnType("decimal(5,2)");
            
            entity.HasOne<AssessmentAttempt>()
                .WithMany(a => a.Answers)
                .HasForeignKey(x => x.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        }
}