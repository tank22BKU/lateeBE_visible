using Microsoft.EntityFrameworkCore;
using VirtualPatientService.Domain.Entities;

namespace VirtualPatientService.Infrastructure.Persistance;

public class VirtualPatientDbContext : DbContext
{
    public VirtualPatientDbContext(DbContextOptions<VirtualPatientDbContext> options)
        : base(options) { }

    public DbSet<VirtualPatient> VirtualPatients => Set<VirtualPatient>();
    public DbSet<ClinicalCase> ClinicalCases => Set<ClinicalCase>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<VirtualPatient>(entity =>
        {
            entity.ToTable("virtual_patient"); 

            entity.HasKey(x => x.PatientId);

            entity.Property(x => x.PatientId).HasColumnName("patient_id").HasMaxLength(50).IsRequired();
            entity.Property(x => x.CaseId).HasColumnName("case_id").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Age).HasColumnName("age");
            entity.Property(x => x.Gender).HasColumnName("gender").HasMaxLength(10);
            entity.Property(x => x.Pronouns).HasColumnName("pronouns").HasMaxLength(50);
            entity.Property(x => x.Occupation).HasColumnName("occupation").HasMaxLength(255);
            entity.Property(x => x.Ethnicity).HasColumnName("ethnicity").HasMaxLength(100);
            entity.Property(x => x.Persona).HasColumnName("persona").HasColumnType("TEXT");
            entity.Property(x => x.ChiefConcern).HasColumnName("chief_concern").HasColumnType("TEXT");
            entity.Property(x => x.VitalSigns).HasColumnName("vital_signs").HasColumnType("TEXT");
            entity.Property(x => x.Instructions).HasColumnName("instructions").HasColumnType("TEXT");
            entity.Property(x => x.Behaviors).HasColumnName("behaviors").HasColumnType("TEXT");
            entity.Property(x => x.TimeSetting).HasColumnName("time_setting");
            entity.Property(x => x.ArgumentTime).HasColumnName("argument_time");
            entity.Property(x => x.LearningObjectives).HasColumnName("learning_objectives").HasColumnType("TEXT");
            entity.Property(x => x.Level).HasColumnName("level").HasMaxLength(20);
            entity.Property(x => x.AvatarImage).HasColumnName("avatar_image").HasMaxLength(255);
            entity.Property(x => x.CaseRule).HasColumnName("case_rule").HasColumnType("TEXT");
            entity.Property(x => x.Status).HasColumnName("status");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        b.Entity<ClinicalCase>(entity =>
        {
            entity.ToTable("clinical_case");

            entity.HasKey(x => x.CaseId);

            entity.Property(x => x.CaseId).HasColumnName("case_id").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description").HasColumnType("TEXT");
            entity.Property(x => x.Symptom).HasColumnName("symptom").HasColumnType("TEXT");
            entity.Property(x => x.MedicalHistory).HasColumnName("medicalhistory").HasColumnType("TEXT");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });
    }
}