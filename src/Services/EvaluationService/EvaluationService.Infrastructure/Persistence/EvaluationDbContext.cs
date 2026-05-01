using Microsoft.EntityFrameworkCore;
using EvaluationService.Domain.Entities;

namespace EvaluationService.Infrastructure.Persistence;

public class EvaluationDbContext : DbContext
{
    public EvaluationDbContext(DbContextOptions<EvaluationDbContext> options) : base(options)
    {}

    public DbSet<EvaluationResult> EvaluationResults => Set<EvaluationResult>();
    public DbSet<EpaScore> EpaScores => Set<EpaScore>();
    public DbSet<EvaluationWarning> EvaluationWarnings => Set<EvaluationWarning>();
    public DbSet<PracticeSession> PracticeSessions => Set<PracticeSession>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<EvaluationResult>(e =>
        {
            e.ToTable("evaluation_results");
            e.HasKey(x => x.ResultId);
            e.Property(x => x.ResultId).HasColumnName("result_id");
            e.Property(x => x.SessionId).HasColumnName("session_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.ClinicalCaseId).HasColumnName("clinical_case_id");
            e.Property(x => x.ModuleId).HasColumnName("module_id");
            e.Property(x => x.CaseType).HasColumnName("case_type");
            e.Property(x => x.DiscussionType).HasColumnName("discussion_type");
            e.Property(x => x.DurationText).HasColumnName("duration_text");
            e.Property(x => x.VpConversationLog).HasColumnName("vp_conversation_log").HasColumnType("json");
            e.Property(x => x.AiReasoningLog).HasColumnName("ai_reasoning_log").HasColumnType("json");
            e.Property(x => x.FinalDiagnosis).HasColumnName("final_diagnosis").HasColumnType("text");
            e.Property(x => x.OverallScore).HasColumnName("overall_score").HasPrecision(5, 2);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");

            e.HasMany(x => x.EpaScores)
                .WithOne(x => x.EvaluationResult)
                .HasForeignKey(x => x.ResultId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Warnings)
                .WithOne(x => x.EvaluationResult)
                .HasForeignKey(x => x.ResultId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<EpaScore>(e =>
        {
            e.ToTable("epa_scores");
            e.HasKey(x => x.ScoreId);
            e.Property(x => x.ScoreId).HasColumnName("score_id");
            e.Property(x => x.ResultId).HasColumnName("result_id");
            e.Property(x => x.EpaId).HasColumnName("epa_id");
            e.Property(x => x.EntrustmentLevel).HasColumnName("entrustment_level");
            e.Property(x => x.NumericalScore).HasColumnName("numerical_score").HasPrecision(5, 2);
            e.Property(x => x.FeedbackDetail).HasColumnName("feedback_detail");
        });

        b.Entity<EvaluationWarning>(e =>
        {
            e.ToTable("evaluation_warnings");
            e.HasKey(x => x.WarningId);
            e.Property(x => x.WarningId).HasColumnName("warning_id");
            e.Property(x => x.ResultId).HasColumnName("result_id");
            e.Property(x => x.WarningType).HasColumnName("label");
            e.Property(x => x.WarningMessage).HasColumnName("description");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        b.Entity<PracticeSession>(entity =>
        {
            entity.ToTable("practice_sessions");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id")
                .HasMaxLength(50)
                .ValueGeneratedNever()
                .IsRequired();

            entity.Property(x => x.LearnerId)
                .HasColumnName("learnerid")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ClinicalCaseId)
                .HasColumnName("clinicalcaseid")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.StartTime)
                .HasColumnName("start_time")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.EndTime)
                .HasColumnName("end_time");

            entity.Property(x => x.Duration)
                .HasColumnName("duration");

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasDefaultValue("Practicing");
        });

    }
}
