using GYM_System.Models;
using System.ComponentModel.DataAnnotations;

namespace GYM_System.ViewModels
{
    public class WorkoutExerciseViewModel
    {
        public int Id { get; set; } // For existing workout exercises

        [Required(ErrorMessage = "Exercise is required.")]
        [Display(Name = "Exercise")]
        public int ExerciseId { get; set; }
        public Exercise? Exercise { get; set; } // Full Exercise object for display (eager-loaded or selected)

        [StringLength(50, ErrorMessage = "Sets cannot exceed 50 characters.")]
        public string? Sets { get; set; } // e.g., "3", "3-4"

        [StringLength(50, ErrorMessage = "Reps cannot exceed 50 characters.")]
        public string? Reps { get; set; } // e.g., "8-12", "10"

        [StringLength(50, ErrorMessage = "Rest cannot exceed 50 characters.")]
        public string? Rest { get; set; } // e.g., "60s", "90-120s"

        [StringLength(50, ErrorMessage = "Tempo cannot exceed 50 characters.")]
        public string? Tempo { get; set; } // e.g., "2-0-1-0"

        [StringLength(50, ErrorMessage = "RPE/RIR cannot exceed 50 characters.")]
        [Display(Name = "RPE/RIR")]
        public string? RpeRir { get; set; } // RPE/RIR, e.g., "RPE 8", "RIR 2"

        [StringLength(1000, ErrorMessage = "Exercise Notes cannot exceed 1000 characters.")]
        [Display(Name = "Notes for Exercise")]
        public string? ExerciseNotes { get; set; }

        // Constructor for a new workout exercise
        public WorkoutExerciseViewModel() { }

        // Constructor for loading existing workout exercises
        public WorkoutExerciseViewModel(WorkoutExercise we)
        {
            Id = we.Id;
            ExerciseId = we.ExerciseId;
            Exercise = we.Exercise; // This assumes Exercise is eager-loaded
            Sets = we.Sets;
            Reps = we.Reps;
            Rest = we.Rest;
            Tempo = we.Tempo;
            RpeRir = we.RpeRir;
            ExerciseNotes = we.ExerciseNotes;
        }
    }
}
