using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_System.Models
{
    public class WorkoutExercise
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int WorkoutDayId { get; set; } // Foreign Key to WorkoutDay
        [ForeignKey("WorkoutDayId")]
        public WorkoutDay? WorkoutDay { get; set; }

        [Required]
        public int ExerciseId { get; set; } // Foreign Key to Exercise
        [ForeignKey("ExerciseId")]
        public Exercise? Exercise { get; set; } // Full Exercise object for display

        [StringLength(50)]
        public string? Sets { get; set; } // e.g., "3", "3-4"

        [StringLength(50)]
        public string? Reps { get; set; } // e.g., "8-12", "10"

        [StringLength(50)]
        public string? Rest { get; set; } // e.g., "60s", "90-120s"

        [StringLength(50)]
        public string? Tempo { get; set; } // e.g., "2-0-1-0"

        [StringLength(50)]
        public string? RpeRir { get; set; } // RPE/RIR, e.g., "RPE 8", "RIR 2"

        [StringLength(1000)]
        public string? ExerciseNotes { get; set; } // Specific notes for this exercise in the plan
    }
}