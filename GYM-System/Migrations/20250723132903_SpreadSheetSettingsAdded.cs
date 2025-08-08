using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYM_System.Migrations
{
    /// <inheritdoc />
    public partial class SpreadSheetSettingsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpreadSheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InitialAssessmentSheetNameAndRange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateFormSheetNameAndRange = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpreadSheets", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpreadSheets");
        }
    }
}
