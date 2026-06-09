using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Expert> Experts => Set<Expert>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.UserId);
            e.Property(x => x.UserId).HasColumnName("userid").HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
            e.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
            e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
            e.Property(x => x.Birthday).HasColumnName("birthday");
            e.Property(x => x.Password).HasColumnName("password").HasMaxLength(255);
            e.Property(x => x.Role).HasColumnName("role").HasMaxLength(20);
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(20);
            e.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(255);
            e.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        });

        b.Entity<Expert>(e =>
        {
            e.ToTable("expert");
            e.HasKey(x => x.ExpertId);
            e.Property(x => x.ExpertId).HasColumnName("eid").HasMaxLength(50).IsRequired();
            e.Property(x => x.Ssn).HasColumnName("ssn").HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Ssn).IsUnique();
            e.Property(x => x.BioQuote).HasColumnName("bio_quote");
            e.Property(x => x.EducationDetail).HasColumnName("education_detail");
            e.Property(x => x.TitlePosition).HasColumnName("title_position").HasMaxLength(255);
            e.Property(x => x.ExpertiseSkill).HasColumnName("expertise_skill");
            e.Property(x => x.SocialLink).HasColumnName("social_link").HasMaxLength(255);
            e.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<Expert>(x => x.ExpertId)
                .HasConstraintName("fk_expert_users");
        });
    }
}
