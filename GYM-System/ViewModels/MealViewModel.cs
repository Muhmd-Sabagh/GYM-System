using GYM_System.Models;
using System.ComponentModel.DataAnnotations;

namespace GYM_System.ViewModels
{
    public class MealViewModel
    {
        public int Id { get; set; } // For existing meals

        [Required(ErrorMessage = "Meal Name is required.")]
        [StringLength(100, ErrorMessage = "Meal Name cannot exceed 100 characters.")]
        [Display(Name = "Meal Name")]
        public string MealName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Meal Notes cannot exceed 1000 characters.")]
        [Display(Name = "Meal Notes")]
        public string? MealNotes { get; set; }

        public List<MealFoodItemViewModel> MealFoodItems { get; set; } = new List<MealFoodItemViewModel>();

        // Calculated properties (NotMapped to DB, populated during UI interaction or when creating PDF)
        public decimal TotalCalories { get; set; }
        public decimal TotalProtein { get; set; }
        public decimal TotalCarbs { get; set; }
        public decimal TotalFat { get; set; }

        // Constructor for a new meal
        public MealViewModel() { }

        // Constructor for loading existing meals
        public MealViewModel(Meal meal)
        {
            Id = meal.Id;
            MealName = meal.MealName;
            MealNotes = meal.MealNotes;

            if (meal.MealFoodItems != null)
            {
                MealFoodItems = meal.MealFoodItems
                                     .OrderBy(mfi => mfi.Id) // Maintain order
                                     .Select(mfi => new MealFoodItemViewModel(mfi))
                                     .ToList();
            }
        }
    }
}