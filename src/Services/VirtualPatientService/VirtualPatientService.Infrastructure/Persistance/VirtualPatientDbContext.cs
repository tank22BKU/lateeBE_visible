using Microsoft.EntityFrameworkCore;
using VirtualPatientService.Domain.Entities;

namespace VirtualPatientService.Infrastructure.Persistance;

public class VirtualPatientDbContext : DbContext
{
    public VirtualPatientDbContext(DbContextOptions<VirtualPatientDbContext> options)
        : base(options) { }

    public DbSet<VirtualPatient> VirtualPatients => Set<VirtualPatient>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<VirtualPatient>(entity =>
        {
            entity.ToTable("patients"); 

            entity.HasKey(x => x.PatientId);

            entity.Property(x => x.PatientId).HasColumnName("patientid").HasMaxLength(50).IsRequired();
            entity.Property(x => x.ClinicalCaseId).HasColumnName("clinical_case_id").HasMaxLength(50);
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(x => x.Age).HasColumnName("age");
            entity.Property(x => x.Gender).HasColumnName("gender").HasMaxLength(20);
            entity.Property(x => x.Pronouns).HasColumnName("pronouns").HasMaxLength(20);
            entity.Property(x => x.Ethnicity).HasColumnName("ethnicity").HasMaxLength(50);
            entity.Property(x => x.Occupation).HasColumnName("occupation").HasMaxLength(100);
            entity.Property(x => x.Setting).HasColumnName("setting").HasMaxLength(50);
            entity.Property(x => x.Level).HasColumnName("level").HasMaxLength(20);
            entity.Property(x => x.TimeSetting).HasColumnName("time_setting").HasMaxLength(50);
            entity.Property(x => x.AvatarImg).HasColumnName("avatar_img").HasColumnType("TEXT");
            entity.Property(x => x.Descriptions).HasColumnName("descriptions").HasColumnType("TEXT");
            entity.Property(x => x.ChiefConcern).HasColumnName("chief_concern").HasColumnType("TEXT");
            
            entity.Property(x => x.VitalSigns).HasColumnName("vital_signs").HasColumnType("JSON");
            entity.Property(x => x.Instructions).HasColumnName("instructions").HasColumnType("JSON");
            entity.Property(x => x.CaseRules).HasColumnName("case_rules").HasColumnType("JSON");
            entity.Property(x => x.Persona).HasColumnName("persona").HasColumnType("JSON");

            entity.Property(x => x.Status).HasColumnName("status");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });
    }
}