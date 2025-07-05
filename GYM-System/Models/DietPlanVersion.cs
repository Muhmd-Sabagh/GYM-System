using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_System.Models
{
    public class DietPlanVersion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int DietPlanId { get; set; } // Foreign Key to DietPlan
        [ForeignKey("DietPlanId")]
        public DietPlan? DietPlan { get; set; }

        [Required]
        [StringLength(100)]
        public string VersionName { get; set; } = string.Empty; // e.g., "High Carb Day", "Low Carb Day"

        [Required]
        public bool IsActiveForPdf { get; set; } = true; // Whether this version should be included in the PDF

        // Navigation property for meals within this version
        public ICollection<Meal>? Meals { get; set; } = new List<Meal>();

        // Optional notes for this specific version
        [StringLength(1000)]
        public string? VersionNotes { get; set; }
    }
}