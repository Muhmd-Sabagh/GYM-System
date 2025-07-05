using GYM_System.Models;
using System.ComponentModel.DataAnnotations;

namespace GYM_System.ViewModels
{
    public class DietPlanVersionViewModel
    {
        public int Id { get; set; } // For existing versions

        [Required(ErrorMessage = "Version Name is required.")]
        [StringLength(100, ErrorMessage = "Version Name cannot exceed 100 characters.")]
        [Display(Name = "Version Name")]
        public string VersionName { get; set; } = string.Empty;

        [Display(Name = "Include in PDF")]
        public bool IsActiveForPdf { get; set; } = true;

        [StringLength(1000, ErrorMessage = "Version Notes cannot exceed 1000 characters.")]
        [Display(Name = "Version Notes")]
        public string? VersionNotes { get; set; }

        public List<MealViewModel> Meals { get; set; } = new List<MealViewModel>();

        // Constructor to initialize a new version with one default meal
        public DietPlanVersionViewModel()
        {
            Meals.Add(new MealViewModel { MealName = "Breakfast" });
        }

        // Constructor for loading existing versions
        public DietPlanVersionViewModel(DietPlanVersion version)
        {
            Id = version.Id;
            VersionName = version.VersionName;
            IsActiveForPdf = version.IsActiveForPdf;
            VersionNotes = version.VersionNotes;

            if (version.Meals != null)
            {
                Meals = version.Meals
                               .OrderBy(m => m.Id) // Maintain order of meals
                               .Select(m => new MealViewModel(m))
                               .ToList();
            }
        }
    }
}