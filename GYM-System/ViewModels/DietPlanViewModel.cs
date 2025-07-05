using GYM_System.Models;
using System.ComponentModel.DataAnnotations;

namespace GYM_System.ViewModels
{
    public class DietPlanViewModel
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

        public List<DietPlanVersionViewModel> Versions { get; set; } = new List<DietPlanVersionViewModel>();

        // Constructor to initialize a new, empty plan with one default version
        public DietPlanViewModel()
        {
            Versions.Add(new DietPlanVersionViewModel { VersionName = "Version 1", IsActiveForPdf = true });
        }

        // Constructor for loading existing plans
        public DietPlanViewModel(DietPlan dietPlan)
        {
            Id = dietPlan.Id;
            PlanName = dietPlan.PlanName;
            ClientId = dietPlan.ClientId;
            GeneralNotes = dietPlan.GeneralNotes;

            if (dietPlan.Versions != null)
            {
                Versions = dietPlan.Versions
                                   .OrderBy(v => v.Id) // Maintain order of versions
                                   .Select(v => new DietPlanVersionViewModel(v))
                                   .ToList();
            }
        }
    }
}