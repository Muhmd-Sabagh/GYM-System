using GYM_System.Models;
using System.ComponentModel.DataAnnotations;

namespace GYM_System.ViewModels
{
    public class MealFoodItemViewModel
    {
        public int Id { get; set; } // For existing meal food items

        [Required(ErrorMessage = "Food Item is required.")]
        public int FoodItemId { get; set; }
        public FoodItem? FoodItem { get; set; } // Full FoodItem object for display (eager-loaded or selected)

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.01, 10000.00, ErrorMessage = "Quantity must be between 0.01 and 10000.")]
        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        // Calculated properties for this item's contribution
        public decimal Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Carbs { get; set; }
        public decimal Fat { get; set; }

        // Constructor for a new meal food item
        public MealFoodItemViewModel() { }

        // Constructor for loading existing meal food items
        public MealFoodItemViewModel(MealFoodItem mfi)
        {
            Id = mfi.Id;
            FoodItemId = mfi.FoodItemId;
            Quantity = mfi.Quantity;
            FoodItem = mfi.FoodItem; // This assumes FoodItem is eager-loaded
        }
    }
}