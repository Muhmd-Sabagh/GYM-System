using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_System.Models
{
    public class WorkoutDay
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int WorkoutPlanId { get; set; } // Foreign Key to WorkoutPlan
        [ForeignKey("WorkoutPlanId")]
        public WorkoutPlan? WorkoutPlan { get; set; }

        [Required]
        [StringLength(100)]
        public string DayName { get; set; } = "Day"; // e.g., "Day 1", "Push Day"

        [StringLength(100)]
        public string? Subtitle { get; set; } // e.g., "Upper Body", "Legs & Core"

        // Navigation property for exercises within this day
        public ICollection<WorkoutExercise>? WorkoutExercises { get; set; } = new List<WorkoutExercise>();

        // Optional notes for this specific workout day
        [StringLength(1000)]
        public string? DayNotes { get; set; }
    }
}