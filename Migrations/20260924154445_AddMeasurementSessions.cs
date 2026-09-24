using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample_Manager_Tool.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurementSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MeasurementSessionId",
                table: "Measurements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MeasurementSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementSessionDevices",
                columns: table => new
                {
                    MeasurementSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementSessionDevices", x => new { x.MeasurementSessionId, x.DeviceId });
                    table.ForeignKey(
                        name: "FK_MeasurementSessionDevices_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MeasurementSessionDevices_MeasurementSessions_MeasurementSessionId",
                        column: x => x.MeasurementSessionId,
                        principalTable: "MeasurementSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_MeasurementSessionId_DeviceId_MeasuredAt",
                table: "Measurements",
                columns: new[] { "MeasurementSessionId", "DeviceId", "MeasuredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementSessionDevices_DeviceId",
                table: "MeasurementSessionDevices",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Measurements_MeasurementSessions_MeasurementSessionId",
                table: "Measurements",
                column: "MeasurementSessionId",
                principalTable: "MeasurementSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Measurements_MeasurementSessions_MeasurementSessionId",
                table: "Measurements");

            migrationBuilder.DropTable(
                name: "MeasurementSessionDevices");

            migrationBuilder.DropTable(
                name: "MeasurementSessions");

            migrationBuilder.DropIndex(
                name: "IX_Measurements_MeasurementSessionId_DeviceId_MeasuredAt",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "MeasurementSessionId",
                table: "Measurements");
        }
    }
}
