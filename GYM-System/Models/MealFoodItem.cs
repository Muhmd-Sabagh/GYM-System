using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_System.Models
{
    public class MealFoodItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int MealId { get; set; } // Foreign Key to Meal
        [ForeignKey("MealId")]
        public Meal? Meal { get; set; }

        [Required]
        public int FoodItemId { get; set; } // Foreign Key to FoodItem
        [ForeignKey("FoodItemId")]
        public FoodItem? FoodItem { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, 10000.00)] // Quantity of the food item in its defined unit
        public decimal Quantity { get; set; }
    }
}