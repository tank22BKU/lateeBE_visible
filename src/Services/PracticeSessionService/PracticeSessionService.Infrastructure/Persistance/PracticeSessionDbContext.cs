using Microsoft.EntityFrameworkCore;
using PracticeSessionService.Domain.Entities;

namespace PracticeSessionService.Infrastructure.Persistance;

public class PracticeSessionDbContext : DbContext
{
    public PracticeSessionDbContext(DbContextOptions<PracticeSessionDbContext> options)
        : base(options)
    {
    }

    public DbSet<PracticeSessionResult> PracticeSessions => Set<PracticeSessionResult>();

    public DbSet<EvaluationWarning> EvaluationWarnings => Set<EvaluationWarning>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<PracticeSessionResult>(entity =>
        {
            entity.ToTable("evaluation_results");

            entity.HasKey(x => x.ResultId);

            entity.Property(x => x.ResultId)
                .HasColumnName("result_id")
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