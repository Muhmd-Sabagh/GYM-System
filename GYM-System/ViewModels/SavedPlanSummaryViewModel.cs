using System.ComponentModel.DataAnnotations;

namespace GYM_System.ViewModels
{
    public class SavedPlanSummaryViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Plan Name")]
        public string PlanName { get; set; } = string.Empty;

        public int? ClientId { get; set; }

        [Display(Name = "Client Name")]
        public string? ClientName { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Plan Type")]
        public PlanType Type { get; set; } // Enum to differentiate Diet vs. Workout

        // For Diet Plans specific filters
        public decimal? TotalCalories { get; set; }
        public decimal? TotalProtein { get; set; }
        public decimal? TotalCarbs { get; set; }
        public decimal? TotalFat { get; set; }

        // For Workout Plans specific filters
        public int? NumberOfDays { get; set; }
    }

    public enum PlanType
    {
        Diet,
        Workout
    }
}
