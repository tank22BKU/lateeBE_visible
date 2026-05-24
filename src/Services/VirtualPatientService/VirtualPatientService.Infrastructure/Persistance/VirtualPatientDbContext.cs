using Microsoft.EntityFrameworkCore;
using VirtualPatientService.Domain.Entities;

namespace VirtualPatientService.Infrastructure.Persistance;

public class VirtualPatientDbContext : DbContext
{
    public VirtualPatientDbContext(DbContextOptions<VirtualPatientDbContext> options)
        : base(options) { }

    public DbSet<VirtualPatient> VirtualPatients => Set<VirtualPatient>();
    public DbSet<ClinicalCase> ClinicalCases => Set<ClinicalCase>();
    public DbSet<Expert> Experts => Set<Expert>();
    public DbSet<ExpertVirtualPatientManagement> ExpertVirtualPatientManagements =>
        Set<ExpertVirtualPatientManagement>();
    public DbSet<LearnerDiscoveryState> LearnerDiscoveryStates => Set<LearnerDiscoveryState>();
    public DbSet<LearnerDiscoveryPool> LearnerDiscoveryPools => Set<LearnerDiscoveryPool>();
    public DbSet<PracticeSessionRef> PracticeSessionRefs => Set<PracticeSessionRef>();
    public DbSet<EvaluationRef> EvaluationRefs => Set<EvaluationRef>();
    public DbSet<UserRef> UserRefs => Set<UserRef>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<VirtualPatient>(e =>
        {
            e.ToTable("virtual_patient");
            e.HasKey(x => x.PatientId);
            e.Property(x => x.PatientId).HasColumnName("patient_id").HasMaxLength(50);
            e.Property(x => x.CaseId).HasColumnName("case_id").HasMaxLength(50);
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
            e.Property(x => x.Age).HasColumnName("age");
            e.Property(x => x.Gender).HasColumnName("gender").HasMaxLength(10);
            e.Property(x => x.Pronouns).HasColumnName("pronouns").HasMaxLength(50);
            e.Property(x => x.Occupation).HasColumnName("occupation").HasMaxLength(255);
            e.Property(x => x.Ethnicity).HasColumnName("ethnicity").HasMaxLength(100);
            e.Property(x => x.Persona).HasColumnName("persona").HasColumnType("TEXT");
            e.Property(x => x.ChiefConcern).HasColumnName("chief_concern").HasMaxLength(255);
            e.Property(x => x.VitalSigns).HasColumnName("vital_signs").HasColumnType("TEXT");
            e.Property(x => x.Instructions).HasColumnName("instructions").HasColumnType("TEXT");
            e.Property(x => x.Behaviors).HasColumnName("behaviors").HasColumnType("TEXT");
            e.Property(x => x.LearningObjectives)
                .HasColumnName("learning_objectives")
                .HasColumnType("TEXT");
            e.Property(x => x.TimeSetting).HasColumnName("time_setting");
            e.Property(x => x.ArgumentTime).HasColumnName("argument_time");
            e.Property(x => x.Level).HasColumnName("level").HasMaxLength(20);
            e.Property(x => x.CaseRule).HasColumnName("case_rule").HasColumnType("TEXT");
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(10);
            e.Property(x => x.AvatarImage).HasColumnName("avatar_image").HasMaxLength(255);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        b.Entity<ClinicalCase>(e =>
        {
            e.ToTable("clinical_case");
            e.HasKey(x => x.CaseId);
            e.Property(x => x.CaseId).HasColumnName("case_id").HasMaxLength(50);
            e.Property(x => x.Title).HasColumnName("title").HasColumnType("TEXT").IsRequired();
            e.Property(x => x.Description).HasColumnName("description").HasColumnType("TEXT");
            e.Property(x => x.Type).HasColumnName("type").HasColumnType("TEXT");
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(50);
            e.Property(x => x.Pe).HasColumnName("pe").HasColumnType("TEXT");
            e.Property(x => x.Symptom).HasColumnName("symptom").HasColumnType("TEXT");
            e.Property(x => x.MedicalHistory).HasColumnName("medicalhistory").HasColumnType("TEXT");
            e.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(50);
            e.Property(x => x.EccId).HasColumnName("eccid").HasMaxLength(50);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        b.Entity<Expert>(e =>
        {
            e.ToTable("expert");
            e.HasKey(x => x.ExpertId);
            e.Property(x => x.ExpertId).HasColumnName("eid").HasMaxLength(50);
            e.Property(x => x.Ssn).HasColumnName("ssn").HasMaxLength(20);
            e.Property(x => x.BioQuote).HasColumnName("bio_quote").HasColumnType("TEXT");
            e.Property(x => x.EducationDetail)
                .HasColumnName("education_detail")
                .HasColumnType("TEXT");
            e.Property(x => x.TitlePosition).HasColumnName("title_position").HasMaxLength(255);
            e.Property(x => x.ExpertiseSkill)
                .HasColumnName("expertise_skill")
                .HasColumnType("TEXT");
            e.Property(x => x.SocialLink).HasColumnName("social_link").HasMaxLength(255);
            e.Ignore(x => x.Name);
            e.Ignore(x => x.AvatarUrl);
            e.Ignore(x => x.Phone);
            e.Ignore(x => x.Email);
        });

        b.Entity<ExpertVirtualPatientManagement>(e =>
        {
            e.ToTable("expert_virtual_patient_management");
            e.HasKey(x => new { x.ExpertId, x.VirtualId });
            e.Property(x => x.ExpertId).HasColumnName("expert_id").HasMaxLength(50);
            e.Property(x => x.VirtualId).HasColumnName("virtual_id").HasMaxLength(50);
        });

        b.Entity<LearnerDiscoveryState>(e =>
        {
            e.ToTable("learner_discovery_state");
            e.HasKey(x => x.LearnerId);
            e.Property(x => x.LearnerId).HasColumnName("learner_id").HasMaxLength(50);
            e.Property(x => x.FilterJson).HasColumnName("filter_json").HasColumnType("json");
            e.Property(x => x.LastAccessed).HasColumnName("last_accessed");
        });

        b.Entity<LearnerDiscoveryPool>(e =>
        {
            e.ToTable("learner_discovery_pool");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.LearnerId, x.PatientId }).IsUnique();
            e.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            e.Property(x => x.LearnerId).HasColumnName("learner_id").HasMaxLength(50);
            e.Property(x => x.PatientId).HasColumnName("patient_id").HasMaxLength(50);
            e.Property(x => x.FetchedAt).HasColumnName("fetched_at");
            e.Property(x => x.FetchLevel).HasColumnName("fetch_level").HasMaxLength(20);
            e.Property(x => x.FetchGender).HasColumnName("fetch_gender").HasMaxLength(10);
            e.HasOne(x => x.VirtualPatient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .HasPrincipalKey(x => x.PatientId);
        });

        b.Entity<PracticeSessionRef>(e =>
        {
            e.ToTable("practice_sessions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasMaxLength(50);
            e.Property(x => x.LearnerId).HasColumnName("learner_id").HasMaxLength(50);
            e.Property(x => x.PatientId).HasColumnName("patient_id").HasMaxLength(50);
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(20);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        b.Entity<EvaluationRef>(e =>
        {
            e.ToTable("evaluation");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasMaxLength(50);
            e.Property(x => x.PracticeSessionId)
                .HasColumnName("practice_session_id")
                .HasMaxLength(50);
            e.Property(x => x.Score).HasColumnName("score").HasColumnType("decimal(5,2)");
        });

        b.Entity<UserRef>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.UserId);
            e.Property(x => x.UserId).HasColumnName("userid").HasMaxLength(50);
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
            e.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(255);
            e.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
            e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
        });
    }
}

public class PracticeSessionRef
{
    public string Id { get; set; } = default!;
    public string LearnerId { get; set; } = default!;
    public string PatientId { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

public class EvaluationRef
{
    public string Id { get; set; } = default!;
    public string PracticeSessionId { get; set; } = default!;
    public decimal? Score { get; set; }
}

public class UserRef
{
    public string UserId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
