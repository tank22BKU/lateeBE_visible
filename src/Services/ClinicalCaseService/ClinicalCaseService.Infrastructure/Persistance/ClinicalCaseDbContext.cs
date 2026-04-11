using Microsoft.EntityFrameworkCore;
using ClinicalCaseService.Domain.Entities;
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
            entity.ToTable("clinicalcases");

            // Primary Key
            entity.HasKey(x => x.ClinicalCaseId);

            entity.Property(x => x.ClinicalCaseId)
                .HasColumnName("clinicalcaseid")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.PatientId)
                .HasColumnName("patientid")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(x => x.Title)
                .HasColumnName("title")
                .HasColumnType("TEXT");

            entity.Property(x => x.CaseType)
                .HasColumnName("type")
                .HasMaxLength(50);

            entity.Property(x => x.Descriptions)
                .HasColumnName("descriptions")
                .HasColumnType("TEXT");

            entity.Property(x => x.Symptom)
                .HasColumnName("symptom")
                .HasColumnType("TEXT");

            entity.Property(x => x.MedicalHistory)
                .HasColumnName("medicalhistory")
                .HasColumnType("TEXT");

            entity.Property(x => x.Pe)
                .HasColumnName("pe")
                .HasColumnType("TEXT");

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(10)
                .HasDefaultValue("active");

            entity.Property(x => x.CreatedBy)
                .HasColumnName("createdBy")
                .HasMaxLength(50);

            entity.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();
        });


        b.Entity<VirtualPatient>(entity =>
        {
            entity.ToTable("patients");

            // Primary Key
            entity.HasKey(x => x.PatientId);

            entity.Property(x => x.PatientId)
                .HasColumnName("patientid")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(x => x.Gender)
                .HasColumnName("gender")
                .HasColumnType("CHAR(1)")
                .IsRequired();

            entity.Property(x => x.Age)
                .HasColumnName("age")
                .IsRequired();

            entity.Property(x => x.Behaviors)
                .HasColumnName("behaviors")
                .HasColumnType("TEXT");

            entity.Property(x => x.Descriptions)
                .HasColumnName("descriptions")
                .HasColumnType("TEXT");

            entity.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();
        });
    }

}