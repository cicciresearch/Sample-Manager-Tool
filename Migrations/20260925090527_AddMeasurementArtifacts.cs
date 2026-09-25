using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample_Manager_Tool.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurementArtifacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SettingsJson",
                table: "Measurements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "darkjvMeasurements",
                columns: table => new
                {
                    MeasurementId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PeakElEqePercent = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_darkjvMeasurements", x => x.MeasurementId);
                    table.ForeignKey(
                        name: "FK_darkjvMeasurements_Measurements_MeasurementId",
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
                name: "darkjvMeasurements");

            migrationBuilder.DropColumn(
                name: "SettingsJson",
                table: "Measurements");
        }
    }
}
