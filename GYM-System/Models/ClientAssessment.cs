using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_System.Models
{
    public class ClientAssessment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; } // Foreign Key to Client
        [ForeignKey("ClientId")]
        public Client? Client { get; set; }

        [Required]
        [Display(Name = "Assessment Date")] // Renamed from Timestamp for clarity
        public DateTime Timestamp { get; set; } // When the form was submitted

        [Required]
        [StringLength(50)]
        public string FormCode { get; set; } = string.Empty; // To link back to the client

        // --- New Initial Assessment Fields based on "Assessment Questions.md" ---

        // Client Information (Questions 2-9 from your list, excluding Code which is ClientId/FormCode)
        [Display(Name = "Country")]
        [StringLength(100)]
        public string? Country { get; set; } // Assuming optional based on typical forms

        [Display(Name = "Religion")]
        [StringLength(50)]
        public string? Religion { get; set; } // مسلم - مسيحي

        [Display(Name = "Goal from Service")]
        [StringLength(500)]
        public string? ServiceGoal { get; set; } // زيادة وزن (تضخيم) - خسارة وزن (تنشيف) - حياة صحية

        [Required(ErrorMessage = "Weight is required.")]
        [Display(Name = "Weight (kg)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal WeightKg { get; set; }

        [Required(ErrorMessage = "Height is required.")]
        [Display(Name = "Height (cm)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal HeightCm { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [Display(Name = "Gender")]
        [StringLength(50)]
        public string Gender { get; set; } // ذكر - انثى

        [Required(ErrorMessage = "Date of Birth is required.")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Job/Profession")]
        [StringLength(255)]
        public string? JobProfession { get; set; } // Optional

        // Body Assessment (Questions 10-17)
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

        // Medical Assessment (Questions 18-26)
        [Display(Name = "Suffer from Health Problems?")]
        public bool HasHealthProblems { get; set; } = false;

        [Display(Name = "Health Problems Details")]
        [StringLength(1000)]
        public string? HealthProblemsDetails { get; set; } // Conditional

        [Display(Name = "Done Tests in Last 6 Months?")]
        public bool HasRecentTests { get; set; } = false;

        [Display(Name = "Recent Tests Details")]
        [StringLength(1000)]
        public string? RecentTestsDetails { get; set; } // Conditional

        [Display(Name = "Taking Medications/Supplements?")]
        public bool IsTakingMedicationsSupplements { get; set; } = false;

        [Display(Name = "Medications/Supplements Details")]
        [StringLength(1000)]
        public string? MedicationsSupplementsDetails { get; set; } // Conditional

        [Display(Name = "Allergy to Certain Medications?")]
        public bool HasMedicationAllergies { get; set; } = false;

        [Display(Name = "Medication Allergies Details")]
        [StringLength(1000)]
        public string? MedicationAllergiesDetails { get; set; } // Conditional

        [Display(Name = "Suffer from Chronic/Hereditary Diseases?")]
        public bool HasChronicHereditaryDiseases { get; set; } = false;

        [Display(Name = "Chronic/Hereditary Diseases Details")]
        [StringLength(1000)]
        public string? ChronicHereditaryDiseasesDetails { get; set; } // Conditional

        [Display(Name = "Undergone Previous Surgeries?")]
        public bool HasPastSurgeries { get; set; } = false;

        [Display(Name = "Past Surgeries Details")]
        [StringLength(1000)]
        public string? PastSurgeriesDetails { get; set; } // Conditional

        [Display(Name = "Pregnant or Planning? (Female Only)")]
        public bool? IsPregnantOrPlanning { get; set; } // (اختياري) - Only for females, so nullable bool

        [Display(Name = "Taking Vitamins/Minerals?")]
        public bool IsTakingVitaminsMinerals { get; set; } = false;

        [Display(Name = "Vitamins/Minerals Details")]
        [StringLength(1000)]
        public string? VitaminsMineralsDetails { get; set; } // Conditional

        [Display(Name = "Other Medical Notes")]
        [StringLength(1000)]
        public string? OtherMedicalNotes { get; set; } // (اختياري)

        // Dietary Assessment (Questions 27-31)
        [Display(Name = "Food/Meal to Remove?")]
        public bool HasFoodToRemove { get; set; } = false;

        [Display(Name = "Food/Meal to Remove Details")]
        [StringLength(1000)]
        public string? FoodToRemoveDetails { get; set; } // Conditional

        [Display(Name = "Food/Meal to Add?")]
        public bool HasFoodToAdd { get; set; } = false;

        [Display(Name = "Food/Meal to Add Details")]
        [StringLength(1000)]
        public string? FoodToAddDetails { get; set; } // Conditional

        [Display(Name = "Food/Meal to Keep From Previous Diet?")]
        public bool HasFoodToKeepFromPrevious { get; set; } = false;

        [Display(Name = "Food/Meal to Keep Details")]
        [StringLength(1000)]
        public string? FoodToKeepFromPreviousDetails { get; set; } // Conditional

        [Required(ErrorMessage = "Desired number of meals is required.")]
        [Display(Name = "Desired Number of Meals")]
        public int DesiredMealsCount { get; set; } // 3 - 6

        [Display(Name = "Dietary Notes")]
        [StringLength(1000)]
        public string? DietaryNotes { get; set; } // (اختياري)

        // Workout Assessment (Questions 32-42)
        [Required(ErrorMessage = "Workout commitment level is required.")]
        [Display(Name = "Workout Commitment Level")]
        [StringLength(100)]
        public string WorkoutCommitmentLevel { get; set; } // غير ملتزم - ملتزم الى حد ما - ملتزم بالكامل

        [Required(ErrorMessage = "Number of workout days per week is required.")]
        [Display(Name = "Number of Workout Days Per Week")]
        public int WorkoutDaysPerWeek { get; set; } // 1 - 7

        [Required(ErrorMessage = "Daily sleep hours is required.")]
        [Display(Name = "Daily Sleep Hours")]
        [StringLength(100)]
        public string DailySleepHours { get; set; } // أقل من 4 ساعات - 4-6 ساعات - 6-8 ساعات - أكثر من 8 ساعات

        [Required(ErrorMessage = "Daily water intake is required.")]
        [Display(Name = "Daily Water Intake (liters)")]
        [StringLength(100)]
        public string DailyWaterIntake { get; set; } // أقل من 2 لتر - 2-3 لتر - 3-4 لتر - أكثر من 4 لتر

        [Required(ErrorMessage = "Daily walking hours is required.")]
        [Display(Name = "Daily Walking Hours")]
        [StringLength(100)]
        public string DailyWalkingHours { get; set; } // لا أمشي - أقل من 30 دقيقة - 30-60 دقيقة - أكثر من 60 دقيقة

        [Display(Name = "Suffer from Previous/Current Injuries?")]
        public bool HasInjuries { get; set; } = false;

        [Display(Name = "Injuries Details")]
        [StringLength(1000)]
        public string? InjuriesDetails { get; set; } // Conditional

        [Display(Name = "Preferred Workout Days (comma-separated)")]
        [StringLength(255)]
        public string? PreferredWorkoutDays { get; set; } // (multiple choices), stored as comma-separated string

        [Display(Name = "Workout Goals (comma-separated)")]
        [StringLength(500)]
        public string? WorkoutGoals { get; set; } // (multiple choices), stored as comma-separated string

        [Display(Name = "Available Equipment (comma-separated)")]
        [StringLength(500)]
        public string? AvailableEquipment { get; set; } // (multiple choices), stored as comma-separated string

        [Display(Name = "Workout Location (comma-separated)")]
        [StringLength(255)]
        public string? WorkoutLocation { get; set; } // (multiple choices), stored as comma-separated string

        [Display(Name = "Workout Notes")]
        [StringLength(1000)]
        public string? WorkoutNotes { get; set; } // (اختياري)
    }
}