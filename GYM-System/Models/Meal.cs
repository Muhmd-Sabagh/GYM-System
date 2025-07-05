using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_System.Models
{
    public class Meal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int DietPlanVersionId { get; set; } // Foreign Key to DietPlanVersion
        [ForeignKey("DietPlanVersionId")]
        public DietPlanVersion? DietPlanVersion { get; set; }

        [Required]
        [StringLength(100)]
        public string MealName { get; set; } = "Meal"; // e.g., "Breakfast", "Lunch", "Snack"

        // Navigation property for food items in this meal
        public ICollection<MealFoodItem>? MealFoodItems { get; set; } = new List<MealFoodItem>();

        // Optional notes for this specific meal
        [StringLength(1000)]
        public string? MealNotes { get; set; }
    }
}