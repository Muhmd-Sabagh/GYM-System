using GYM_System.Models;
using System.ComponentModel.DataAnnotations;

namespace GYM_System.ViewModels
{
    public class WorkoutDayViewModel
    {
        public int Id { get; set; } // For existing days

        [Required(ErrorMessage = "Day Name is required.")]
        [StringLength(100, ErrorMessage = "Day Name cannot exceed 100 characters.")]
        [Display(Name = "Day Name")]
        public string DayName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Subtitle cannot exceed 100 characters.")]
        [Display(Name = "Subtitle (e.g., Push, Pull, Legs)")]
        public string? Subtitle { get; set; }

        [StringLength(1000, ErrorMessage = "Day Notes cannot exceed 1000 characters.")]
        [Display(Name = "Day Notes")]
        public string? DayNotes { get; set; }

        public List<WorkoutExerciseViewModel> WorkoutExercises { get; set; } = new List<WorkoutExerciseViewModel>();

        // Constructor for a new workout day
        public WorkoutDayViewModel() { }

        // Constructor for loading existing workout days
        public WorkoutDayViewModel(WorkoutDay workoutDay)
        {
            Id = workoutDay.Id;
            DayName = workoutDay.DayName;
            Subtitle = workoutDay.Subtitle;
            DayNotes = workoutDay.DayNotes;

            if (workoutDay.WorkoutExercises != null)
            {
                WorkoutExercises = workoutDay.WorkoutExercises
                                            .OrderBy(we => we.Id) // Maintain order
                                            .Select(we => new WorkoutExerciseViewModel(we))
                                            .ToList();
            }
        }
    }
}