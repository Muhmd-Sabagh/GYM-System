using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_System.Models
{
    public class ClientUpdate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; } // Foreign Key to Client
        [ForeignKey("ClientId")]
        public Client? Client { get; set; }

        [Required]
        [Display(Name = "Update Date")]
        public DateTime Timestamp { get; set; } // When the form was submitted

        [Required]
        [StringLength(50)]
        [Display(Name = "Form Code")]
        public string FormCode { get; set; } = string.Empty; // To link back to the client

        // --- Update Form Fields ---
        [Required(ErrorMessage = "Current weight is required.")]
        [Display(Name = "Current Weight (kg)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CurrentWeightKg { get; set; }

        // Dietary Assessment Update (Questions 3-11)
        [Required(ErrorMessage = "Diet commitment level is required.")]
        [Display(Name = "Diet Commitment Level")]
        [StringLength(100)]
        public string DietCommitmentLevel { get; set; } = string.Empty; // غير ملتزم - ملتزم الى حد ما - ملتزم تماما

        [Required(ErrorMessage = "Food/meal to remove status is required.")]
        [Display(Name = "Food/Meal to Remove?")]
        public bool HasFoodToRemove { get; set; }

        [Display(Name = "Food/Meal to Remove Details")]
        [StringLength(1000)]
        public string? FoodToRemoveDetails { get; set; } // (اختياري)

        [Required(ErrorMessage = "Food/meal to add status is required.")]
        [Display(Name = "Food/Meal to Add?")]
        public bool HasFoodToAdd { get; set; }

        [Display(Name = "Food/Meal to Add Details")]
        [StringLength(1000)]
        public string? FoodToAddDetails { get; set; } // (اختياري)

        [Required(ErrorMessage = "Food/meal to keep status is required.")]
        [Display(Name = "Food/Meal to Keep From Previous Diet?")]
        public bool HasFoodToKeepFromPrevious { get; set; }

        [Display(Name = "Food/Meal to Keep Details")]
        [StringLength(1000)]
        public string? FoodToKeepFromPreviousDetails { get; set; } // (اختياري)

        [Required(ErrorMessage = "Desired number of meals is required.")]
        [Display(Name = "Desired Number of Meals")]
        public int DesiredMealsCount { get; set; } // 3 - 4 - 5 - 6

        [Display(Name = "Dietary Notes")]
        [StringLength(1000)]
        public string? DietaryNotes { get; set; } // (اختياري)

        // Body Assessment Update (Questions 12-19)
        [Display(Name = "Neck Circumference (cm)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? NeckCircumferenceCm { get; set; } // (اختياري)

        [Display(Name = "Waist Circumference (cm)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? WaistCircumferenceCm { get; set; } // (اختياري)

        [Display(Name = "Hip Circumference (cm)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? HipCircumferenceCm { get; set; } // (اختياري)

        [Display(Name = "Arm Circumference (cm)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ArmCircumferenceCm { get; set; } // (اختياري)

        [Display(Name = "Thigh Circumference (cm)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ThighCircumferenceCm { get; set; } // (اختياري)

        [Display(Name = "Front Body Photo Path")]
        [StringLength(500)]
        public string? FrontBodyPhotoPath { get; set; } // (اختياري) - URL for uploaded images

        [Display(Name = "Side Body Photo Path")]
        [StringLength(500)]
        public string? SideBodyPhotoPath { get; set; } // (اختياري) - URL for uploaded images

        [Display(Name = "Back Body Photo Path")]
        [StringLength(500)]
        public string? BackBodyPhotoPath { get; set; } // (اختياري) - URL for uploaded images

        // Workout Assessment Update (Questions 20-31)
        [Required(ErrorMessage = "Workout commitment level is required.")]
        [Display(Name = "Workout Commitment Level")]
        [StringLength(100)]
        public string WorkoutCommitmentLevel { get; set; } = string.Empty; // غير ملتزم - ملتزم الى حد ما - ملتزم بالكامل

        [Required(ErrorMessage = "Previous workout system experience is required.")]
        [Display(Name = "Previous Workout System Experience")]
        [StringLength(1000)]
        public string PreviousWorkoutSystemExperience { get; set; } = string.Empty;

        [Required(ErrorMessage = "Weight/reps development status is required.")]
        [Display(Name = "Development in Weights/Reps?")]
        public bool HasWeightRepsDevelopment { get; set; }

        [Required(ErrorMessage = "Training volume suitability status is required.")]
        [Display(Name = "Training Volume Suitable?")]
        public bool IsTrainingVolumeSuitable { get; set; }

        [Display(Name = "Desired Training Volume Adjustment")]
        [StringLength(100)]
        public string? DesiredTrainingVolumeAdjustment { get; set; } // (اختياري) - حجم اقل - حجم أعلى

        [Required(ErrorMessage = "Training intensity suitability status is required.")]
        [Display(Name = "Training Intensity Suitable?")]
        public bool IsTrainingIntensitySuitable { get; set; }

        [Display(Name = "Desired Training Intensity Adjustment")]
        [StringLength(100)]
        public string? DesiredTrainingIntensityAdjustment { get; set; } // (اختياري) - شدة اقل - شدة أعلى

        [Required(ErrorMessage = "Exercise discomfort status is required.")]
        [Display(Name = "Exercises causing pain/discomfort?")]
        public bool HasExerciseDiscomfort { get; set; }

        [Display(Name = "Discomfort Exercise Name")]
        [StringLength(1000)]
        public string? DiscomfortExerciseName { get; set; } // (اختياري)

        [Required(ErrorMessage = "Available workout days count is required.")]
        [Display(Name = "Available Workout Days Count")]
        public int AvailableWorkoutDaysCount { get; set; } // 1 - 6

        [Required(ErrorMessage = "Workout location is required.")]
        [Display(Name = "Workout Location")]
        [StringLength(100)]
        public string WorkoutLocation { get; set; } = string.Empty; // جيم - منزل

        [Display(Name = "Available Home Equipment")]
        [StringLength(1000)]
        public string? AvailableHomeEquipment { get; set; } // (اختياري)

        [Display(Name = "Notes")]
        [StringLength(1000)]
        public string? Notes { get; set; } // (اختياري) - General notes for the update
    }
}