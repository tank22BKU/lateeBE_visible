using Microsoft.EntityFrameworkCore;
using RoadmapService.Domain.Entities;

namespace RoadmapService.Infrastructure.Persistence;

public class RoadmapDbContext : DbContext
{
    public RoadmapDbContext(DbContextOptions<RoadmapDbContext> options)
        : base(options) { }
    public DbSet<Roadmap> Roadmaps => Set<Roadmap>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Roadmap>(entity =>
        {
            entity.ToTable("roadmaps");

            entity.HasKey(x => x.RoadmapId);

            entity.Property(x => x.RoadmapId)
                .HasColumnName("id")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.LearnerId)
                .HasColumnName("learner_id")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Content)
                .HasColumnName("content")
                .HasColumnType("JSON")
                .IsRequired();

            entity.Property(x => x.Version)
                .HasColumnName("version")
                .HasMaxLength(20);

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}