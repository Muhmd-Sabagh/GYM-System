namespace GYM_System.ViewModels
{
    #region PDF-Specific ViewModels (for Base64 image and font embedding)

    /// <summary>
    /// ViewModel specifically for Diet Plan PDF generation with Base64 images and fonts.
    /// </summary>
    public class DietPlanPdfViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string? GeneralNotes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LogoBase64 { get; set; } = string.Empty;
        public bool ShowVersionHeaders { get; set; }
        public List<DietPlanVersionPdfViewModel> Versions { get; set; } = new();

        // Font Base64 strings for embedding in HTML
        public string TajawalRegularBase64 { get; set; } = string.Empty;
        public string TajawalMediumBase64 { get; set; } = string.Empty;
        public string TajawalBoldBase64 { get; set; } = string.Empty;
    }

    public class DietPlanVersionPdfViewModel
    {
        public string VersionName { get; set; } = string.Empty;
        public string? VersionNotes { get; set; }
        public decimal TotalCalories { get; set; }
        public decimal TotalProtein { get; set; }
        public decimal TotalCarbs { get; set; }
        public decimal TotalFat { get; set; }
        public List<MealPdfViewModel> Meals { get; set; } = new();
    }

    public class MealPdfViewModel
    {
        public string MealName { get; set; } = string.Empty;
        public string? MealNotes { get; set; }
        public decimal TotalCalories { get; set; }
        public decimal TotalProtein { get; set; }
        public decimal TotalCarbs { get; set; }
        public decimal TotalFat { get; set; }
        public List<MealFoodItemPdfViewModel> FoodItems { get; set; } = new();
    }

    public class MealFoodItemPdfViewModel
    {
        public string FoodItemName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string ImageBase64 { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel specifically for Workout Plan PDF generation with Base64 images and fonts.
    /// </summary>
    public class WorkoutPlanPdfViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string? GeneralNotes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LogoBase64 { get; set; } = string.Empty;
        public int TotalDays { get; set; }
        public int TotalExercises { get; set; }
        public List<WorkoutDayPdfViewModel> WorkoutDays { get; set; } = new();

        // Font Base64 strings for embedding in HTML
        public string TajawalRegularBase64 { get; set; } = string.Empty;
        public string TajawalMediumBase64 { get; set; } = string.Empty;
        public string TajawalBoldBase64 { get; set; } = string.Empty;
    }

    public class WorkoutDayPdfViewModel
    {
        public string DayName { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? DayNotes { get; set; }
        public int ExerciseCount { get; set; }
        public List<WorkoutExercisePdfViewModel> Exercises { get; set; } = new();
    }

    public class WorkoutExercisePdfViewModel
    {
        public string ExerciseName { get; set; } = string.Empty;
        public string Sets { get; set; } = "-";
        public string Reps { get; set; } = "-";
        public string Rest { get; set; } = "-";
        public string Tempo { get; set; } = "-";
        public string RpeRir { get; set; } = "-";
        public string? ExerciseNotes { get; set; }
        public string? YouTubeLink { get; set; }
    }

    #endregion
}
