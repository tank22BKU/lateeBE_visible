using Microsoft.EntityFrameworkCore;
using KnowledgeResourceService.Domain.Entities;

namespace KnowledgeResourceService.Infrastructure.Persistence;

public class KnowledgeDbContext : DbContext
{
    public KnowledgeDbContext(DbContextOptions<KnowledgeDbContext> options) : base(options) { }

    public DbSet<KnowledgeResource> KnowledgeResources => Set<KnowledgeResource>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<KnowledgeResource>(e =>
        {
            e.ToTable("knowledge_resources");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasMaxLength(50).IsRequired();
            e.Property(x => x.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            e.Property(x => x.Content).HasColumnName("content").HasColumnType("TEXT");
            e.Property(x => x.AuthorId).HasColumnName("author_id").HasMaxLength(50);
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
