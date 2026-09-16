using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample_Manager_Tool.Migrations
{
    /// <inheritdoc />
    public partial class AddEIS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EisMeasurements",
                columns: table => new
                {
                    MeasurementId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PeakFrequencyHz = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EisMeasurements", x => x.MeasurementId);
                    table.ForeignKey(
                        name: "FK_EisMeasurements_Measurements_MeasurementId",
                        column: x => x.MeasurementId,
                        principalTable: "Measurements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EisMeasurements");
        }
    }
}
