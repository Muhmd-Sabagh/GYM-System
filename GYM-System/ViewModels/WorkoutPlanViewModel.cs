using GYM_System.Models;
using System.ComponentModel.DataAnnotations;

namespace GYM_System.ViewModels
{
    public class WorkoutPlanViewModel
    {
        public int Id { get; set; } // For existing plans

        [Required(ErrorMessage = "Plan Name is required.")]
        [StringLength(255, ErrorMessage = "Plan Name cannot exceed 255 characters.")]
        [Display(Name = "Plan Name (Internal)")]
        public string PlanName { get; set; } = string.Empty;

        [Display(Name = "Client (Optional)")]
        public int? ClientId { get; set; }
        public Client? Client { get; set; } // For displaying client name in UI

        [StringLength(1000, ErrorMessage = "General Notes cannot exceed 1000 characters.")]
        [Display(Name = "General Notes for Plan")]
        public string? GeneralNotes { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public List<WorkoutDayViewModel> WorkoutDays { get; set; } = new List<WorkoutDayViewModel>();

        // Constructor to initialize a new, empty plan with one default day
        public WorkoutPlanViewModel()
        {
            WorkoutDays.Add(new WorkoutDayViewModel { DayName = "Day 1" });
        }

        // Constructor for loading existing plans
        public WorkoutPlanViewModel(WorkoutPlan workoutPlan)
        {
            Id = workoutPlan.Id;
            PlanName = workoutPlan.PlanName;
            ClientId = workoutPlan.ClientId;
            Client = workoutPlan.Client;
            CreatedDate = workoutPlan.CreatedDate;
            GeneralNotes = workoutPlan.GeneralNotes;

            if (workoutPlan.WorkoutDays != null)
            {
                WorkoutDays = workoutPlan.WorkoutDays
                                         .OrderBy(wd => wd.Id) // Maintain order of days
                                         .Select(wd => new WorkoutDayViewModel(wd))
                                         .ToList();
            }
        }
    }
}