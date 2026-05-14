using Microsoft.EntityFrameworkCore;
using EvaluationService.Domain.Entities;
using System.Text.Json;

namespace EvaluationService.Infrastructure.Persistence;

public class EvaluationDbContext : DbContext
{
    public EvaluationDbContext(DbContextOptions<EvaluationDbContext> options)
        : base(options) { }

    public DbSet<Evaluation>          Evaluations       => Set<Evaluation>();
    public DbSet<EvaluationEpaScore>  EpaScores         => Set<EvaluationEpaScore>();
    public DbSet<Warning>             Warnings          => Set<Warning>();
    public DbSet<PracticeSession>     PracticeSessions  => Set<PracticeSession>();
    public DbSet<PracticeFeedback>    PracticeFeedbacks => Set<PracticeFeedback>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // static ValueComparer<List<string>> ListComparer() =>
        //     new(
        //         (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
        //         c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
        //         c => c.ToList()
        //     );
            
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
            // e.Property(x => x.FeedbackDetail).HasColumnName("feedback_detail");
            e.Property(x => x.EntrustmentLevel).HasColumnName("entrustment_level");
            e.Property(x => x.RubricVersion).HasColumnName("rubric_version").HasMaxLength(20);
            e.HasMany(x => x.EpaScores)
                .WithOne(x => x.Evaluation)
                .HasForeignKey(x => x.EvaluationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<EvaluationEpaScore>(e =>
        {
            e.ToTable("evaluation_epa_score");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasMaxLength(50);
            e.Property(x => x.EvaluationId).HasColumnName("evaluation_id").HasMaxLength(50).IsRequired();
            e.Property(x => x.EpaId).HasColumnName("epa_id").HasMaxLength(20).IsRequired();
            e.Property(x => x.NumericalScore).HasColumnName("numerical_score");
            e.Property(x => x.EntrustmentLevel).HasColumnName("entrustment_level");
            e.Property(x => x.FeedbackDetail).HasColumnName("feedback_detail");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.EvidenceCited)
                .HasColumnName("evidence_cited")
                .HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );
            e.Property(x => x.FailurePatterns)
                .HasColumnName("failure_patterns")
                .HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new()
                );
            e.Property(x => x.SafetyFlags)
                .HasColumnName("safety_flags")
                .HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new()
                );
        });

        b.Entity<Warning>(e =>
        {
            e.ToTable("warning");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.PracticeSessionId).HasColumnName("practice_session_id").IsRequired();;
            e.Property(x => x.LearnerId).HasColumnName("learner_id").IsRequired();;
            e.Property(x => x.Label).HasColumnName("label");
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        b.Entity<PracticeSession>(e =>
        {
            e.ToTable("practice_sessions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasMaxLength(50).ValueGeneratedNever().IsRequired();
            e.Property(x => x.LearnerId).HasColumnName("learner_id").HasMaxLength(50).IsRequired();
            e.Property(x => x.PatientId).HasColumnName("patient_id").HasMaxLength(50).IsRequired();
            e.Property(x => x.FinalDiagnosis).HasColumnName("final_diagnosis");
            e.Property(x => x.AiReasoningLog).HasColumnName("ai_reasoning_log").HasColumnType("json");
            e.Property(x => x.VpConversationLog).HasColumnName("vp_conversation_log").HasColumnType("json");
            e.Property(x => x.ModuleId).HasColumnName("module_id");
            e.Property(x => x.DiscussionType).HasColumnName("discussion_type");
            e.Property(x => x.GuidelinesId).HasColumnName("guidelines_id");
            e.Property(x => x.StartTime).HasColumnName("start_time").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.EndTime).HasColumnName("end_time");
            e.Property(x => x.Status).HasColumnName("status").HasDefaultValue("Practicing");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        b.Entity<PracticeFeedback>(e =>
        {
            e.ToTable("practice_feedback");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasMaxLength(50);
            e.Property(x => x.OverallAttempt).HasColumnName("overall_attempt");
            e.Property(x => x.OverallLabel).HasColumnName("overall_label");
            e.Property(x => x.Strength).HasColumnName("strength");
            e.Property(x => x.Improvement).HasColumnName("improvement");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            e.Property(x => x.EvaluationId).HasColumnName("evaluation_id").HasMaxLength(50).IsRequired();
            e.Property(x => x.PracticeSessionId).HasColumnName("practice_session_id").HasMaxLength(50).IsRequired();
        });
    }
}