using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYM_System.Migrations
{
    /// <inheritdoc />
    public partial class GoogleFormAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FormCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HeightCm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ActivityLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DietaryPreferences = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FitnessGoals = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OtherNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientAssessments_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FormCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CurrentWeightKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PhysiqueChanges = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DietAdjustmentsNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExerciseAdjustmentsNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientUpdates_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientAssessments_ClientId",
                table: "ClientAssessments",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientUpdates_ClientId",
                table: "ClientUpdates",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientAssessments");

            migrationBuilder.DropTable(
                name: "ClientUpdates");
        }
    }
}
