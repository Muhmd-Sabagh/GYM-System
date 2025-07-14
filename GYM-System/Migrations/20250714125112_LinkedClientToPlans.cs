using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYM_System.Migrations
{
    /// <inheritdoc />
    public partial class LinkedClientToPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DietAdjustmentsNotes",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "ClientAssessments");

            migrationBuilder.RenameColumn(
                name: "UploadedImageUrl",
                table: "ClientUpdates",
                newName: "SideBodyPhotoPath");

            migrationBuilder.RenameColumn(
                name: "PhysiqueChanges",
                table: "ClientUpdates",
                newName: "FrontBodyPhotoPath");

            migrationBuilder.RenameColumn(
                name: "ExerciseAdjustmentsNotes",
                table: "ClientUpdates",
                newName: "BackBodyPhotoPath");

            migrationBuilder.RenameColumn(
                name: "CurrentWeightKg",
                table: "ClientUpdates",
                newName: "WaistCircumferenceCm");

            migrationBuilder.RenameColumn(
                name: "AdditionalNotes",
                table: "ClientUpdates",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "UploadedImageUrl",
                table: "ClientAssessments",
                newName: "WorkoutGoals");

            migrationBuilder.RenameColumn(
                name: "OtherNotes",
                table: "ClientAssessments",
                newName: "WorkoutNotes");

            migrationBuilder.RenameColumn(
                name: "FitnessGoals",
                table: "ClientAssessments",
                newName: "SideBodyPhotoPath");

            migrationBuilder.RenameColumn(
                name: "DietaryPreferences",
                table: "ClientAssessments",
                newName: "ServiceGoal");

            migrationBuilder.RenameColumn(
                name: "ActivityLevel",
                table: "ClientAssessments",
                newName: "Country");

            migrationBuilder.AddColumn<decimal>(
                name: "ArmCircumferenceCm",
                table: "ClientUpdates",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HipCircumferenceCm",
                table: "ClientUpdates",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NeckCircumferenceCm",
                table: "ClientUpdates",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ThighCircumferenceCm",
                table: "ClientUpdates",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkoutCommitmentLevel",
                table: "ClientUpdates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "WeightKg",
                table: "ClientAssessments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "HeightCm",
                table: "ClientAssessments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "ClientAssessments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ArmCircumferenceCm",
                table: "ClientAssessments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvailableEquipment",
                table: "ClientAssessments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackBodyPhotoPath",
                table: "ClientAssessments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChronicHereditaryDiseasesDetails",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DailySleepHours",
                table: "ClientAssessments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DailyWalkingHours",
                table: "ClientAssessments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DailyWaterIntake",
                table: "ClientAssessments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "ClientAssessments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DesiredMealsCount",
                table: "ClientAssessments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DietaryNotes",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FoodToAddDetails",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FoodToKeepFromPreviousDetails",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FoodToRemoveDetails",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrontBodyPhotoPath",
                table: "ClientAssessments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasChronicHereditaryDiseases",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasFoodToAdd",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasFoodToKeepFromPrevious",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasFoodToRemove",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasHealthProblems",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasInjuries",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasMedicationAllergies",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPastSurgeries",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasRecentTests",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HealthProblemsDetails",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HipCircumferenceCm",
                table: "ClientAssessments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InjuriesDetails",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPregnantOrPlanning",
                table: "ClientAssessments",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTakingMedicationsSupplements",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTakingVitaminsMinerals",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JobProfession",
                table: "ClientAssessments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicationAllergiesDetails",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicationsSupplementsDetails",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NeckCircumferenceCm",
                table: "ClientAssessments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherMedicalNotes",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PastSurgeriesDetails",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredWorkoutDays",
                table: "ClientAssessments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecentTestsDetails",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Religion",
                table: "ClientAssessments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ThighCircumferenceCm",
                table: "ClientAssessments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VitaminsMineralsDetails",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WaistCircumferenceCm",
                table: "ClientAssessments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkoutCommitmentLevel",
                table: "ClientAssessments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WorkoutDaysPerWeek",
                table: "ClientAssessments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WorkoutLocation",
                table: "ClientAssessments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArmCircumferenceCm",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "HipCircumferenceCm",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "NeckCircumferenceCm",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "ThighCircumferenceCm",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "WorkoutCommitmentLevel",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "ArmCircumferenceCm",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "AvailableEquipment",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "BackBodyPhotoPath",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "ChronicHereditaryDiseasesDetails",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "DailySleepHours",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "DailyWalkingHours",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "DailyWaterIntake",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "DesiredMealsCount",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "DietaryNotes",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "FoodToAddDetails",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "FoodToKeepFromPreviousDetails",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "FoodToRemoveDetails",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "FrontBodyPhotoPath",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "HasChronicHereditaryDiseases",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "HasFoodToAdd",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "HasFoodToKeepFromPrevious",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "HasFoodToRemove",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "HasHealthProblems",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "HasInjuries",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "HasMedicationAllergies",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "HasPastSurgeries",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "HasRecentTests",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "HealthProblemsDetails",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "HipCircumferenceCm",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "InjuriesDetails",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "IsPregnantOrPlanning",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "IsTakingMedicationsSupplements",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "IsTakingVitaminsMinerals",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "JobProfession",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "MedicationAllergiesDetails",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "MedicationsSupplementsDetails",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "NeckCircumferenceCm",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "OtherMedicalNotes",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "PastSurgeriesDetails",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "PreferredWorkoutDays",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "RecentTestsDetails",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "Religion",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "ThighCircumferenceCm",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "VitaminsMineralsDetails",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "WaistCircumferenceCm",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "WorkoutCommitmentLevel",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "WorkoutDaysPerWeek",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "WorkoutLocation",
                table: "ClientAssessments");

            migrationBuilder.RenameColumn(
                name: "WaistCircumferenceCm",
                table: "ClientUpdates",
                newName: "CurrentWeightKg");

            migrationBuilder.RenameColumn(
                name: "SideBodyPhotoPath",
                table: "ClientUpdates",
                newName: "UploadedImageUrl");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "ClientUpdates",
                newName: "AdditionalNotes");

            migrationBuilder.RenameColumn(
                name: "FrontBodyPhotoPath",
                table: "ClientUpdates",
                newName: "PhysiqueChanges");

            migrationBuilder.RenameColumn(
                name: "BackBodyPhotoPath",
                table: "ClientUpdates",
                newName: "ExerciseAdjustmentsNotes");

            migrationBuilder.RenameColumn(
                name: "WorkoutNotes",
                table: "ClientAssessments",
                newName: "OtherNotes");

            migrationBuilder.RenameColumn(
                name: "WorkoutGoals",
                table: "ClientAssessments",
                newName: "UploadedImageUrl");

            migrationBuilder.RenameColumn(
                name: "SideBodyPhotoPath",
                table: "ClientAssessments",
                newName: "FitnessGoals");

            migrationBuilder.RenameColumn(
                name: "ServiceGoal",
                table: "ClientAssessments",
                newName: "DietaryPreferences");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "ClientAssessments",
                newName: "ActivityLevel");

            migrationBuilder.AddColumn<string>(
                name: "DietAdjustmentsNotes",
                table: "ClientUpdates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "WeightKg",
                table: "ClientAssessments",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "HeightCm",
                table: "ClientAssessments",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "ClientAssessments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "ClientAssessments",
                type: "int",
                nullable: true);
        }
    }
}
