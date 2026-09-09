using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample_Manager_Tool.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Measurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    MeasuredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataPath = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Measurements_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EqeMeasurements",
                columns: table => new
                {
                    MeasurementId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IntegratedJscMilliampPerCm2 = table.Column<double>(type: "REAL", nullable: true),
                    PeakEqePercent = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EqeMeasurements", x => x.MeasurementId);
                    table.ForeignKey(
                        name: "FK_EqeMeasurements_Measurements_MeasurementId",
                        column: x => x.MeasurementId,
                        principalTable: "Measurements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JvMeasurements",
                columns: table => new
                {
                    MeasurementId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VocV = table.Column<double>(type: "REAL", nullable: true),
                    JscMilliampPerCm2 = table.Column<double>(type: "REAL", nullable: true),
                    FillFactorPercent = table.Column<double>(type: "REAL", nullable: true),
                    EfficiencyPercent = table.Column<double>(type: "REAL", nullable: true),
                    VmppV = table.Column<double>(type: "REAL", nullable: true),
                    JmppMilliampPerCm2 = table.Column<double>(type: "REAL", nullable: true),
                    PmppMilliwattPerCm2 = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JvMeasurements", x => x.MeasurementId);
                    table.ForeignKey(
                        name: "FK_JvMeasurements_Measurements_MeasurementId",
                        column: x => x.MeasurementId,
                        principalTable: "Measurements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_DeviceId_Type_MeasuredAt",
                table: "Measurements",
                columns: new[] { "DeviceId", "Type", "MeasuredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EqeMeasurements");

            migrationBuilder.DropTable(
                name: "JvMeasurements");

            migrationBuilder.DropTable(
                name: "Measurements");
        }
    }
}
