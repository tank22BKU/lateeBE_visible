using Microsoft.EntityFrameworkCore;
using PracticeSessionService.Domain.Entities;

namespace PracticeSessionService.Infrastructure.Persistance;

public class PracticeSessionDbContext : DbContext
{
    public PracticeSessionDbContext(DbContextOptions<PracticeSessionDbContext> options)
        : base(options)
    {
    }

    public DbSet<PracticeSession> PracticeSessions => Set<PracticeSession>();
    public DbSet<PracticeSessionResult> EvaluationResults => Set<PracticeSessionResult>();
    public DbSet<EvaluationWarning> EvaluationWarnings => Set<EvaluationWarning>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

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

            entity.HasMany(x => x.EvaluationResults)
                .WithOne()
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<PracticeSessionResult>(entity =>
        {
            entity.ToTable("evaluation_results");

            entity.HasKey(x => x.ResultId);

            entity.Property(x => x.ResultId)
                .HasColumnName("result_id")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.SessionId)
                .HasColumnName("session_id")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.UserId)
                .HasColumnName("user_id")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ClinicalCaseId)
                .HasColumnName("clinical_case_id")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ModuleId)
                .HasColumnName("module_id")
                .HasMaxLength(50)
                .HasDefaultValue("EPA_STANDARD_V1");

            entity.Property(x => x.VpConversationLog)
                .HasColumnName("vp_conversation_log")
                .HasColumnType("json");

            entity.Property(x => x.AiReasoningLog)
                .HasColumnName("ai_reasoning_log")
                .HasColumnType("json");

            entity.Property(x => x.FinalDiagnosis)
                .HasColumnName("final_diagnosis")
                .HasColumnType("text");

            entity.Property(x => x.OverallScore)
                .HasColumnName("overall_score")
                .HasPrecision(5, 2);

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasMany(e => e.Warnings)
                .WithOne(w => w.PracticeSessionResult)
                .HasForeignKey(w => w.ResultId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<EvaluationWarning>(entity =>
        {
            entity.ToTable("evaluation_warnings");

            entity.HasKey(e => e.WarningId);

            entity.Property(e => e.WarningId)
                .HasColumnName("warning_id");

            entity.Property(e => e.ResultId)
                .HasColumnName("result_id");

            entity.Property(e => e.Label)
                .HasColumnName("label");

            entity.Property(e => e.Description)
                .HasColumnName("description");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
        });
    }
}