using Microsoft.EntityFrameworkCore;
using EvaluationService.Domain.Entities;

namespace Evaluation.Service.Infracstructure.Persistence;

public class EvaluationDbContext : DbContext
{
    public EvaluationDbContext(DbContextOptions<EvaluationDbContext> options) : base(options)
    {}

    public DbSet<EvaluationResult> EvaluationResults { get; set; } = null!;
    public DbSet<EpaScore> EpaScores { get; set; } = null!;
    public DbSet<EvaluationWarning> EvaluationWarnings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder b) {
        b.Entity<EvaluationResult>(e => {
            e.ToTable("evaluation_results");
            e.HasKey(x => x.ResultId);
            e.Property(x => x.VpConversationLog).HasColumnType("JSON");
            e.Property(x => x.AiReasoningLog).HasColumnType("JSON");
        });
        b.Entity<EpaScore>(e => {
            e.ToTable("epa_scores");
            e.Property(x => x.NumericalScore).HasPrecision(5, 2);
        });
    }
}
