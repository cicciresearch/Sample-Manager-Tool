using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample_Manager_Tool.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEqeParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PeakEqePercent",
                table: "EqeMeasurements",
                newName: "PeakEQE");

            migrationBuilder.RenameColumn(
                name: "IntegratedJscMilliampPerCm2",
                table: "EqeMeasurements",
                newName: "Jsc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PeakEQE",
                table: "EqeMeasurements",
                newName: "PeakEqePercent");

            migrationBuilder.RenameColumn(
                name: "Jsc",
                table: "EqeMeasurements",
                newName: "IntegratedJscMilliampPerCm2");
        }
    }
}
