using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYM_System.Migrations
{
    /// <inheritdoc />
    public partial class ClientAssessmentsUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableEquipment",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "IsPregnantOrPlanning",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "PreferredWorkoutDays",
                table: "ClientAssessments");

            migrationBuilder.RenameColumn(
                name: "WorkoutNotes",
                table: "ClientAssessments",
                newName: "PreviousOnlineTrainingExperience");

            migrationBuilder.RenameColumn(
                name: "WorkoutGoals",
                table: "ClientAssessments",
                newName: "LastDietSystemAvailable");

            migrationBuilder.RenameColumn(
                name: "WorkoutDaysPerWeek",
                table: "ClientAssessments",
                newName: "DesiredMainMealsCount");

            migrationBuilder.RenameColumn(
                name: "WorkoutCommitmentLevel",
                table: "ClientAssessments",
                newName: "ResistanceTrainingDuration");

            migrationBuilder.RenameColumn(
                name: "VitaminsMineralsDetails",
                table: "ClientAssessments",
                newName: "PreviousDietExperience");

            migrationBuilder.RenameColumn(
                name: "OtherMedicalNotes",
                table: "ClientAssessments",
                newName: "OtherSportsDetails");

            migrationBuilder.RenameColumn(
                name: "IsTakingVitaminsMinerals",
                table: "ClientAssessments",
                newName: "WantsVitaminsMinerals");

            migrationBuilder.RenameColumn(
                name: "HasFoodToRemove",
                table: "ClientAssessments",
                newName: "PracticesOtherSports");

            migrationBuilder.RenameColumn(
                name: "HasFoodToKeepFromPrevious",
                table: "ClientAssessments",
                newName: "IsSmoker");

            migrationBuilder.RenameColumn(
                name: "HasFoodToAdd",
                table: "ClientAssessments",
                newName: "HasPreviousDietCommitment");

            migrationBuilder.RenameColumn(
                name: "FoodToRemoveDetails",
                table: "ClientAssessments",
                newName: "OtherNotes");

            migrationBuilder.RenameColumn(
                name: "FoodToKeepFromPreviousDetails",
                table: "ClientAssessments",
                newName: "FoodAllergyDetails");

            migrationBuilder.RenameColumn(
                name: "FoodToAddDetails",
                table: "ClientAssessments",
                newName: "DislikedFoodDetails");

            migrationBuilder.RenameColumn(
                name: "DietaryNotes",
                table: "ClientAssessments",
                newName: "DiscomfortExercisesDetails");

            migrationBuilder.RenameColumn(
                name: "DesiredMealsCount",
                table: "ClientAssessments",
                newName: "AvailableWorkoutDaysCount");

            migrationBuilder.RenameColumn(
                name: "DailyWaterIntake",
                table: "ClientAssessments",
                newName: "PreferredCardioType");

            migrationBuilder.RenameColumn(
                name: "DailyWalkingHours",
                table: "ClientAssessments",
                newName: "DietFlexibilityPreference");

            migrationBuilder.RenameColumn(
                name: "DailySleepHours",
                table: "ClientAssessments",
                newName: "DietBudget");

            migrationBuilder.AlterColumn<string>(
                name: "WorkoutCommitmentLevel",
                table: "ClientUpdates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvailableHomeEquipment",
                table: "ClientUpdates",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AvailableWorkoutDaysCount",
                table: "ClientUpdates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentWeightKg",
                table: "ClientUpdates",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DesiredMealsCount",
                table: "ClientUpdates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DesiredTrainingIntensityAdjustment",
                table: "ClientUpdates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesiredTrainingVolumeAdjustment",
                table: "ClientUpdates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DietCommitmentLevel",
                table: "ClientUpdates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DietaryNotes",
                table: "ClientUpdates",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscomfortExerciseName",
                table: "ClientUpdates",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FoodToAddDetails",
                table: "ClientUpdates",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FoodToKeepFromPreviousDetails",
                table: "ClientUpdates",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FoodToRemoveDetails",
                table: "ClientUpdates",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasExerciseDiscomfort",
                table: "ClientUpdates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasFoodToAdd",
                table: "ClientUpdates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasFoodToKeepFromPrevious",
                table: "ClientUpdates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasFoodToRemove",
                table: "ClientUpdates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasWeightRepsDevelopment",
                table: "ClientUpdates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrainingIntensitySuitable",
                table: "ClientUpdates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrainingVolumeSuitable",
                table: "ClientUpdates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreviousWorkoutSystemExperience",
                table: "ClientUpdates",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkoutLocation",
                table: "ClientUpdates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "WorkoutLocation",
                table: "ClientAssessments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ServiceGoal",
                table: "ClientAssessments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Religion",
                table: "ClientAssessments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "JobProfession",
                table: "ClientAssessments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "ClientAssessments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvailableDesiredVitaminsMinerals",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvailableHomeEquipment",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvailableWorkoutDays",
                table: "ClientAssessments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BeverageConsumptionDetails",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DailyEffortDescription",
                table: "ClientAssessments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DailyStepsCount",
                table: "ClientAssessments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DietCommitmentObstacles",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "DislikesSpecificFood",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DrinksSpecificBeverages",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasExerciseDiscomfort",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasFoodAllergies",
                table: "ClientAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreferredCarbohydrateTypes",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferredHealthyFatTypes",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferredProteinTypes",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReasonForSubscription",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkoutExperience",
                table: "ClientAssessments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableHomeEquipment",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "AvailableWorkoutDaysCount",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "CurrentWeightKg",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "DesiredMealsCount",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "DesiredTrainingIntensityAdjustment",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "DesiredTrainingVolumeAdjustment",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "DietCommitmentLevel",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "DietaryNotes",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "DiscomfortExerciseName",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "FoodToAddDetails",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "FoodToKeepFromPreviousDetails",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "FoodToRemoveDetails",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "HasExerciseDiscomfort",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "HasFoodToAdd",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "HasFoodToKeepFromPrevious",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "HasFoodToRemove",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "HasWeightRepsDevelopment",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "IsTrainingIntensitySuitable",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "IsTrainingVolumeSuitable",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "PreviousWorkoutSystemExperience",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "WorkoutLocation",
                table: "ClientUpdates");

            migrationBuilder.DropColumn(
                name: "AvailableDesiredVitaminsMinerals",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "AvailableHomeEquipment",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "AvailableWorkoutDays",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "BeverageConsumptionDetails",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "DailyEffortDescription",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "DailyStepsCount",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "DietCommitmentObstacles",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "DislikesSpecificFood",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "DrinksSpecificBeverages",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "HasExerciseDiscomfort",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "HasFoodAllergies",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "PreferredCarbohydrateTypes",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "PreferredHealthyFatTypes",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "PreferredProteinTypes",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "ReasonForSubscription",
                table: "ClientAssessments");

            migrationBuilder.DropColumn(
                name: "WorkoutExperience",
                table: "ClientAssessments");

            migrationBuilder.RenameColumn(
                name: "WantsVitaminsMinerals",
                table: "ClientAssessments",
                newName: "IsTakingVitaminsMinerals");

            migrationBuilder.RenameColumn(
                name: "ResistanceTrainingDuration",
                table: "ClientAssessments",
                newName: "WorkoutCommitmentLevel");

            migrationBuilder.RenameColumn(
                name: "PreviousOnlineTrainingExperience",
                table: "ClientAssessments",
                newName: "WorkoutNotes");

            migrationBuilder.RenameColumn(
                name: "PreviousDietExperience",
                table: "ClientAssessments",
                newName: "VitaminsMineralsDetails");

            migrationBuilder.RenameColumn(
                name: "PreferredCardioType",
                table: "ClientAssessments",
                newName: "DailyWaterIntake");

            migrationBuilder.RenameColumn(
                name: "PracticesOtherSports",
                table: "ClientAssessments",
                newName: "HasFoodToRemove");

            migrationBuilder.RenameColumn(
                name: "OtherSportsDetails",
                table: "ClientAssessments",
                newName: "OtherMedicalNotes");

            migrationBuilder.RenameColumn(
                name: "OtherNotes",
                table: "ClientAssessments",
                newName: "FoodToRemoveDetails");

            migrationBuilder.RenameColumn(
                name: "LastDietSystemAvailable",
                table: "ClientAssessments",
                newName: "WorkoutGoals");

            migrationBuilder.RenameColumn(
                name: "IsSmoker",
                table: "ClientAssessments",
                newName: "HasFoodToKeepFromPrevious");

            migrationBuilder.RenameColumn(
                name: "HasPreviousDietCommitment",
                table: "ClientAssessments",
                newName: "HasFoodToAdd");

            migrationBuilder.RenameColumn(
                name: "FoodAllergyDetails",
                table: "ClientAssessments",
                newName: "FoodToKeepFromPreviousDetails");

            migrationBuilder.RenameColumn(
                name: "DislikedFoodDetails",
                table: "ClientAssessments",
                newName: "FoodToAddDetails");

            migrationBuilder.RenameColumn(
                name: "DiscomfortExercisesDetails",
                table: "ClientAssessments",
                newName: "DietaryNotes");

            migrationBuilder.RenameColumn(
                name: "DietFlexibilityPreference",
                table: "ClientAssessments",
                newName: "DailyWalkingHours");

            migrationBuilder.RenameColumn(
                name: "DietBudget",
                table: "ClientAssessments",
                newName: "DailySleepHours");

            migrationBuilder.RenameColumn(
                name: "DesiredMainMealsCount",
                table: "ClientAssessments",
                newName: "WorkoutDaysPerWeek");

            migrationBuilder.RenameColumn(
                name: "AvailableWorkoutDaysCount",
                table: "ClientAssessments",
                newName: "DesiredMealsCount");

            migrationBuilder.AlterColumn<string>(
                name: "WorkoutCommitmentLevel",
                table: "ClientUpdates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "WorkoutLocation",
                table: "ClientAssessments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ServiceGoal",
                table: "ClientAssessments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Religion",
                table: "ClientAssessments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "JobProfession",
                table: "ClientAssessments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "ClientAssessments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "AvailableEquipment",
                table: "ClientAssessments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPregnantOrPlanning",
                table: "ClientAssessments",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredWorkoutDays",
                table: "ClientAssessments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
