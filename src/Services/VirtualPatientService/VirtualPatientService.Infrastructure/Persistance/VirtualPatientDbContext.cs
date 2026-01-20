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

            entity.Property(x => x.Description)
                .HasColumnName("description")
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