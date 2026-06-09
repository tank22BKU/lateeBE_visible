using Microsoft.EntityFrameworkCore;
using PracticeSessionService.Domain.Entities;

namespace PracticeSessionService.Infrastructure.Persistance;

public class PracticeSessionDbContext : DbContext
{
    public PracticeSessionDbContext(DbContextOptions<PracticeSessionDbContext> options)
        : base(options) { }

    public DbSet<PracticeSession> PracticeSessions => Set<PracticeSession>();
    public DbSet<Warning> Warnings => Set<Warning>();
    public DbSet<ClinicalCase> ClinicalCases => Set<ClinicalCase>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<PracticeSession>(entity =>
        {
            entity.ToTable("practice_sessions");
            entity.HasKey(x => x.Id);

            entity
                .Property(x => x.Id)
                .HasColumnName("id")
                .HasMaxLength(50)
                .ValueGeneratedNever()
                .IsRequired();

            entity
                .Property(x => x.LearnerId)
                .HasColumnName("learner_id")
                .HasMaxLength(50)
                .IsRequired();

            entity
                .Property(x => x.PatientId)
                .HasColumnName("patient_id")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.FinalDiagnosis).HasColumnName("final_diagnosis");

            entity
                .Property(x => x.AiReasoningLog)
                .HasColumnName("ai_reasoning_log")
                .HasColumnType("json");

            entity
                .Property(x => x.VpConversationLog)
                .HasColumnName("vp_conversation_log")
                .HasColumnType("json");

            entity.Property(x => x.ModuleId).HasColumnName("module_id");

            entity.Property(x => x.DiscussionType).HasColumnName("discussion_type");

            entity.Property(x => x.GuidelinesId).HasColumnName("guidelines_id");

            entity
                .Property(x => x.StartTime)
                .HasColumnName("start_time")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.EndTime).HasColumnName("end_time");

            entity.Property(x => x.Status).HasColumnName("status").HasDefaultValue("Practicing");

            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        b.Entity<Warning>(entity =>
        {
            entity.ToTable("warning");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.PracticeSessionId).HasColumnName("practice_session_id");

            entity.Property(e => e.LearnerId).HasColumnName("learner_id");

            entity.Property(e => e.Label).HasColumnName("label");

            entity.Property(e => e.Description).HasColumnName("description");

            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        b.Entity<ClinicalCase>(entity =>
        {
            entity.ToTable("clinical_case");

            entity.HasKey(e => e.CaseId);

            entity.Property(e => e.CaseId).HasColumnName("case_id").HasMaxLength(50).IsRequired();

            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();

            entity.Property(e => e.Description).HasColumnName("description").HasColumnType("TEXT");

            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(50);

            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);

            entity.Property(e => e.Pe).HasColumnName("pe").HasColumnType("TEXT");

            entity.Property(e => e.Symptom).HasColumnName("symptom").HasColumnType("TEXT");

            entity
                .Property(e => e.MedicalHistory)
                .HasColumnName("medicalhistory")
                .HasColumnType("TEXT");

            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(50);

            entity.Property(e => e.EccId).HasColumnName("eccid").HasMaxLength(50);

            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });
    }
}
