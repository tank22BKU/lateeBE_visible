using ClinicalCaseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using VirtualPatientService.Domain.Entities;

namespace ClinicalCaseService.Infrastructure.Persistence;

public class ClinicalCaseDbContext : DbContext
{
    public ClinicalCaseDbContext(DbContextOptions<ClinicalCaseDbContext> options)
        : base(options) { }

    public DbSet<ClinicalCase> ClinicalCases => Set<ClinicalCase>();
    public DbSet<VirtualPatient> VirtualPatients => Set<VirtualPatient>();

    // public ClinicalCase getFirstClinicalCase()
    // {
    //     return _context.ClinicalCase.Where(c => c.Status == "active").FirstOrDefault();
    // }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<ClinicalCase>(entity =>
        {
            entity.ToTable("clinical_case");

            entity.HasKey(x => x.CaseId);

            entity.Property(x => x.CaseId).HasColumnName("case_id").HasMaxLength(50).IsRequired();

            entity.Property(x => x.Title).HasColumnName("title").HasColumnType("TEXT").IsRequired();

            entity.Property(x => x.Description).HasColumnName("description").HasColumnType("TEXT");

            entity.Property(x => x.CaseType).HasColumnName("type").HasColumnType("TEXT");

            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(50);

            entity.Property(x => x.Symptom).HasColumnName("symptom").HasColumnType("TEXT");

            entity
                .Property(x => x.MedicalHistory)
                .HasColumnName("medicalhistory")
                .HasColumnType("TEXT");

            entity.Property(x => x.Pe).HasColumnName("pe").HasColumnType("TEXT");

            entity
                .Property(x => x.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.EccId).HasColumnName("eccid").HasMaxLength(50).IsRequired();

            entity
                .Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity
                .Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();
        });

        b.Entity<VirtualPatient>(entity =>
        {
            entity.ToTable("virtual_patient");
            entity.HasKey(x => x.PatientId);

            entity
                .Property(x => x.PatientId)
                .HasColumnName("patient_id")
                .HasMaxLength(50)
                .IsRequired();

            entity
                .Property(x => x.Gender)
                .HasColumnName("gender")
                .HasColumnType("CHAR(1)")
                .IsRequired();

            entity.Property(x => x.Age).HasColumnName("age").IsRequired();

            entity.Property(x => x.Behaviors).HasColumnName("behaviors").HasColumnType("TEXT");

            entity
                .Property(x => x.Descriptions)
                .HasColumnName("descriptions")
                .HasColumnType("TEXT");

            entity.Property(x => x.CreatedAt).HasColumnName("created_at");

            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });
    }
}
