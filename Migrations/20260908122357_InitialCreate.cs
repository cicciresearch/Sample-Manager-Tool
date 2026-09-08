using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample_Manager_Tool.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceStacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceStacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResearchUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StackLayers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceStackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    Material = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StackLayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StackLayers_DeviceStacks_DeviceStackId",
                        column: x => x.DeviceStackId,
                        principalTable: "DeviceStacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Area = table.Column<float>(type: "REAL", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ProductionDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeviceStackId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Batches_DeviceStacks_DeviceStackId",
                        column: x => x.DeviceStackId,
                        principalTable: "DeviceStacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Batches_ResearchUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ResearchUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Samples",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BatchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Samples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Samples_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Pixel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SampleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_Samples_SampleId",
                        column: x => x.SampleId,
                        principalTable: "Samples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Batches_Code",
                table: "Batches",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Batches_DeviceStackId",
                table: "Batches",
                column: "DeviceStackId");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_UserId",
                table: "Batches",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_SampleId_Pixel",
                table: "Devices",
                columns: new[] { "SampleId", "Pixel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResearchUsers_Name",
                table: "ResearchUsers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Samples_BatchId_Code",
                table: "Samples",
                columns: new[] { "BatchId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StackLayers_DeviceStackId_Position",
                table: "StackLayers",
                columns: new[] { "DeviceStackId", "Position" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "StackLayers");

            migrationBuilder.DropTable(
                name: "Samples");

            migrationBuilder.DropTable(
                name: "Batches");

            migrationBuilder.DropTable(
                name: "DeviceStacks");

            migrationBuilder.DropTable(
                name: "ResearchUsers");
        }
    }
}
