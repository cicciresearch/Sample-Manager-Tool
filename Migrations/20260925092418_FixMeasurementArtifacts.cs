using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample_Manager_Tool.Migrations
{
    /// <inheritdoc />
    public partial class FixMeasurementArtifacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeasurementArtifacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MeasurementId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kind = table.Column<string>(
                        type: "TEXT",
                        maxLength: 100,
                        nullable: false),
                    Path = table.Column<string>(
                        type: "TEXT",
                        maxLength: 2000,
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_MeasurementArtifacts",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_MeasurementArtifacts_Measurements_MeasurementId",
                        column: x => x.MeasurementId,
                        principalTable: "Measurements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementArtifacts_MeasurementId_Kind",
                table: "MeasurementArtifacts",
                columns: new[]
                {
            "MeasurementId",
            "Kind"
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeasurementArtifacts");
        }
    }
}
