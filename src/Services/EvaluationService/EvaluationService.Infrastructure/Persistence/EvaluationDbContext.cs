using Microsoft.EntityFrameworkCore;
using EvaluationService.Domain.Entities;

namespace EvaluationService.Infrastructure.Persistence;

public class EvaluationDbContext : DbContext
{
    public EvaluationDbContext(DbContextOptions<EvaluationDbContext> options) : base(options)
    {}

    public DbSet<Evaluation> Evaluations => Set<Evaluation>();
    public DbSet<Warning> Warnings => Set<Warning>();
    public DbSet<PracticeSession> PracticeSessions => Set<PracticeSession>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Evaluation>(e =>
        {
            e.ToTable("evaluation");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.EpaId).HasColumnName("epa_id");
            e.Property(x => x.PracticeSessionId).HasColumnName("practice_session_id");
            e.Property(x => x.Score).HasColumnName("score").HasPrecision(5, 2);
            e.Property(x => x.Duration).HasColumnName("duration");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.FeedbackDetail).HasColumnName("feedback_detail");
            e.Property(x => x.EntrustmentLevel).HasColumnName("entrustment_level");
        });

        b.Entity<Warning>(e =>
        {
            e.ToTable("warning");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.PracticeSessionId).HasColumnName("practice_session_id");
            e.Property(x => x.LearnerId).HasColumnName("learner_id");
            e.Property(x => x.Label).HasColumnName("label");
            e.Property(x => x.Description).HasColumnName("description");
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
                .HasColumnName("learner_id")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.PatientId)
                .HasColumnName("patient_id")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.FinalDiagnosis)
                .HasColumnName("final_diagnosis");

            entity.Property(x => x.AiReasoningLog)
                .HasColumnName("ai_reasoning_log")
                .HasColumnType("json");

            entity.Property(x => x.VpConversationLog)
                .HasColumnName("vp_conversation_log")
                .HasColumnType("json");

            entity.Property(x => x.ModuleId)
                .HasColumnName("module_id");

            entity.Property(x => x.DiscussionType)
                .HasColumnName("discussion_type");

            entity.Property(x => x.GuidelinesId)
                .HasColumnName("guidelines_id");

            entity.Property(x => x.StartTime)
                .HasColumnName("start_time")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.EndTime)
                .HasColumnName("end_time");

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasDefaultValue("Practicing");

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at");
        });

    }
}
