using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYM_System.Migrations
{
    /// <inheritdoc />
    public partial class DbContextUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DietPlans_Clients_ClientId",
                table: "DietPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutPlans_Clients_ClientId",
                table: "WorkoutPlans");

            migrationBuilder.AddForeignKey(
                name: "FK_DietPlans_Clients_ClientId",
                table: "DietPlans",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutPlans_Clients_ClientId",
                table: "WorkoutPlans",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DietPlans_Clients_ClientId",
                table: "DietPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutPlans_Clients_ClientId",
                table: "WorkoutPlans");

            migrationBuilder.AddForeignKey(
                name: "FK_DietPlans_Clients_ClientId",
                table: "DietPlans",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutPlans_Clients_ClientId",
                table: "WorkoutPlans",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");
        }
    }
}
