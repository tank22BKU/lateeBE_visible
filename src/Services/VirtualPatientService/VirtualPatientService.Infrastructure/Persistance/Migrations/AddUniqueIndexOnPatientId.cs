using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtualPatientService.Infrastructure.Persistance.Migrations;

public partial class AddUniqueIndexOnPatientId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_virtual_patient_patient_id",
            table: "virtual_patient",
            column: "patient_id",
            unique: true
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_virtual_patient_patient_id", table: "virtual_patient");
    }
}
