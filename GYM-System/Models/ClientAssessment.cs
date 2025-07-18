using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System; // For DateTime
using System.Collections.Generic; // For ICollection if needed elsewhere, though not directly for this model's properties

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
        [Display(Name = "Assessment Date")]
        public DateTime Timestamp { get; set; } // When the form was submitted

        [Required]
        [StringLength(50)]
        [Display(Name = "Form Code")]
        public string FormCode { get; set; } = string.Empty; // To link back to the client

        // --- Client Information (Questions 2-9) ---
        [Required(ErrorMessage = "Country is required.")]
        [Display(Name = "Country")]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Religion is required.")]
        [Display(Name = "Religion")]
        [StringLength(50)]
        public string Religion { get; set; } = string.Empty; // مسلم - مسيحي

        [Required(ErrorMessage = "Goal from Service is required.")]
        [Display(Name = "Goal from Service")]
        [StringLength(500)]
        public string ServiceGoal { get; set; } = string.Empty; // زيادة وزن (تضخيم) - خسارة وزن (تنشيف) - حياة صحية

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
        public string Gender { get; set; } = string.Empty; // ذكر - انثى

        [Required(ErrorMessage = "Date of Birth is required.")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Job/Profession is required.")]
        [Display(Name = "Job/Profession")]
        [StringLength(255)]
        public string JobProfession { get; set; } = string.Empty;

        // --- Body Assessment (Questions 10-17) ---
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

        // --- Medical Assessment (Questions 18-32) ---
        [Required(ErrorMessage = "Health problems status is required.")]
        [Display(Name = "Suffer from Health Problems?")]
        public bool HasHealthProblems { get; set; }

        [Display(Name = "Health Problems Details")]
        [StringLength(1000)]
        public string? HealthProblemsDetails { get; set; } // (اختياري)

        [Required(ErrorMessage = "Recent tests status is required.")]
        [Display(Name = "Done Tests in Last 6 Months?")]
        public bool HasRecentTests { get; set; }

        [Display(Name = "Recent Tests Details")]
        [StringLength(1000)]
        public string? RecentTestsDetails { get; set; } // (اختياري)

        [Required(ErrorMessage = "Medication/supplements usage status is required.")]
        [Display(Name = "Taking Medications/Supplements?")]
        public bool IsTakingMedicationsSupplements { get; set; }

        [Display(Name = "Medications/Supplements Details")]
        [StringLength(1000)]
        public string? MedicationsSupplementsDetails { get; set; } // (اختياري)

        [Required(ErrorMessage = "Medication allergy status is required.")]
        [Display(Name = "Allergy to Certain Medications?")]
        public bool HasMedicationAllergies { get; set; }

        [Display(Name = "Medication Allergies Details")]
        [StringLength(1000)]
        public string? MedicationAllergiesDetails { get; set; } // (اختياري)

        [Required(ErrorMessage = "Chronic/hereditary diseases status is required.")]
        [Display(Name = "Suffer from Chronic/Hereditary Diseases?")]
        public bool HasChronicHereditaryDiseases { get; set; }

        [Display(Name = "Chronic/Hereditary Diseases Details")]
        [StringLength(1000)]
        public string? ChronicHereditaryDiseasesDetails { get; set; } // (اختياري)

        [Required(ErrorMessage = "Previous surgeries status is required.")]
        [Display(Name = "Undergone Previous Surgeries?")]
        public bool HasPastSurgeries { get; set; }

        [Display(Name = "Past Surgeries Details")]
        [StringLength(1000)]
        public string? PastSurgeriesDetails { get; set; } // (اختياري)

        [Required(ErrorMessage = "Injuries status is required.")]
        [Display(Name = "Do you have any injuries?")]
        public bool HasInjuries { get; set; }

        [Display(Name = "Injuries Details")]
        [StringLength(1000)]
        public string? InjuriesDetails { get; set; } // (اختياري)

        [Required(ErrorMessage = "Smoking status is required.")]
        [Display(Name = "Are you a smoker?")]
        public bool IsSmoker { get; set; } // لا، أنا لا أدخن - نعم، أنا مدخن

        // --- Dietary Assessment (Questions 33-51) ---
        [Required(ErrorMessage = "Previous diet commitment status is required.")]
        [Display(Name = "Committed to a diet before?")]
        public bool HasPreviousDietCommitment { get; set; }

        [Display(Name = "Previous Diet Experience")]
        [StringLength(1000)]
        public string? PreviousDietExperience { get; set; } // (اختياري)

        [Required(ErrorMessage = "Daily effort description is required.")]
        [Display(Name = "Daily Effort Description")]
        [StringLength(500)]
        public string DailyEffortDescription { get; set; } = string.Empty; // لا يوجد مجهود تقريبا - مجهود بسيط - مجهود متوسط - مجهود عالي

        [Required(ErrorMessage = "Reasons for not sticking to diet are required.")]
        [Display(Name = "Reasons for not sticking to diet")]
        [StringLength(1000)]
        public string DietCommitmentObstacles { get; set; } = string.Empty;

        [Required(ErrorMessage = "Continuous beverage consumption status is required.")]
        [Display(Name = "Drink specific beverages continuously?")]
        public bool DrinksSpecificBeverages { get; set; }

        [Display(Name = "Beverage Consumption Details")]
        [StringLength(1000)]
        public string? BeverageConsumptionDetails { get; set; } // (اختياري)

        [Display(Name = "Last Diet System (if available)")]
        [StringLength(500)]
        public string? LastDietSystemAvailable { get; set; } // (اختياري)

        [Required(ErrorMessage = "Food allergy status is required.")]
        [Display(Name = "Allergy to any food type?")]
        public bool HasFoodAllergies { get; set; }

        [Display(Name = "Food Allergy Details")]
        [StringLength(1000)]
        public string? FoodAllergyDetails { get; set; } // (اختياري)

        [Required(ErrorMessage = "Disliked food status is required.")]
        [Display(Name = "Dislike any specific food?")]
        public bool DislikesSpecificFood { get; set; }

        [Display(Name = "Disliked Food Details")]
        [StringLength(1000)]
        public string? DislikedFoodDetails { get; set; } // (اختياري)

        [Required(ErrorMessage = "Vitamins/minerals usage desire is required.")]
        [Display(Name = "Want to use vitamins/minerals?")]
        public bool WantsVitaminsMinerals { get; set; }

        [Display(Name = "Available/Desired Vitamins/Minerals")]
        [StringLength(1000)]
        public string? AvailableDesiredVitaminsMinerals { get; set; } // (اختياري)

        [Required(ErrorMessage = "Desired number of main meals is required.")]
        [Display(Name = "Desired Number of Main Meals")]
        public int DesiredMainMealsCount { get; set; } // 3 - 6

        [Required(ErrorMessage = "Diet flexibility preference is required.")]
        [Display(Name = "Diet Flexibility Preference")]
        [StringLength(100)]
        public string DietFlexibilityPreference { get; set; } = string.Empty; // أريد مرونة في النظام الغذائي - مستعد لأقسو على نفسي

        [Required(ErrorMessage = "Diet budget is required.")]
        [Display(Name = "Diet Budget")]
        [StringLength(100)]
        public string DietBudget { get; set; } = string.Empty; // ميزانية منخفضة - ميزانية متوسطة - ميزانية عالية

        [Required(ErrorMessage = "Preferred protein types are required.")]
        [Display(Name = "Preferred Protein Types (comma-separated)")]
        [StringLength(1000)]
        public string PreferredProteinTypes { get; set; } = string.Empty; // (multiple choices)

        [Required(ErrorMessage = "Preferred carbohydrate types are required.")]
        [Display(Name = "Preferred Carbohydrate Types (comma-separated)")]
        [StringLength(1000)]
        public string PreferredCarbohydrateTypes { get; set; } = string.Empty; // (multiple choices)

        [Required(ErrorMessage = "Preferred healthy fat types are required.")]
        [Display(Name = "Preferred Healthy Fat Types (comma-separated)")]
        [StringLength(1000)]
        public string PreferredHealthyFatTypes { get; set; } = string.Empty; // (multiple choices)

        // --- Workout Assessment (Questions 52-63) ---
        [Required(ErrorMessage = "Workout experience is required.")]
        [Display(Name = "Workout Experience")]
        [StringLength(1000)]
        public string WorkoutExperience { get; set; } = string.Empty;

        [Required(ErrorMessage = "Resistance training duration is required.")]
        [Display(Name = "Resistance Training Duration")]
        [StringLength(100)]
        public string ResistanceTrainingDuration { get; set; } = string.Empty; // أول مرة - أقل من 3 شهور - أكثر من 3 شهور - أكثر من 6 شهور - أكثر من سنة

        [Required(ErrorMessage = "Other sports status is required.")]
        [Display(Name = "Practice other sports regularly?")]
        public bool PracticesOtherSports { get; set; }

        [Display(Name = "Other Sports Details")]
        [StringLength(1000)]
        public string? OtherSportsDetails { get; set; } // (اختياري)

        [Required(ErrorMessage = "Workout location is required.")]
        [Display(Name = "Workout Location")]
        [StringLength(100)]
        public string WorkoutLocation { get; set; } = string.Empty; // جيم - منزل

        [Display(Name = "Available Home Equipment")]
        [StringLength(1000)]
        public string? AvailableHomeEquipment { get; set; } // (اختياري)

        [Required(ErrorMessage = "Available workout days count is required.")]
        [Display(Name = "Available Workout Days Count")]
        public int AvailableWorkoutDaysCount { get; set; } // 1 - 6

        [Required(ErrorMessage = "Available workout days are required.")]
        [Display(Name = "Available Workout Days (comma-separated)")]
        [StringLength(255)]
        public string AvailableWorkoutDays { get; set; } = string.Empty; // (multiple choice)

        [Required(ErrorMessage = "Exercise discomfort status is required.")]
        [Display(Name = "Exercises causing pain/discomfort?")]
        public bool HasExerciseDiscomfort { get; set; }

        [Display(Name = "Discomfort Exercises Details")]
        [StringLength(1000)]
        public string? DiscomfortExercisesDetails { get; set; } // (اختياري)

        [Required(ErrorMessage = "Preferred cardio type is required.")]
        [Display(Name = "Preferred Cardio Type")]
        [StringLength(100)]
        public string PreferredCardioType { get; set; } = string.Empty; // تريدميل - عجلة - سلم - تجديف - other

        [Required(ErrorMessage = "Daily steps count is required.")]
        [Display(Name = "Daily Steps Count")]
        [StringLength(100)] // Storing as string to match potential varied input (e.g., "10,000+")
        public string DailyStepsCount { get; set; } = string.Empty;

        // --- General Assessment (Questions 64-66) ---
        [Display(Name = "Previous Online Training Experience")]
        [StringLength(1000)]
        public string? PreviousOnlineTrainingExperience { get; set; } // (اختياري)

        [Required(ErrorMessage = "Reason for subscription is required.")]
        [Display(Name = "Reason for Subscription")]
        [StringLength(1000)]
        public string ReasonForSubscription { get; set; } = string.Empty;

        [Display(Name = "Other Notes")]
        [StringLength(1000)]
        public string? OtherNotes { get; set; } // (اختياري)
    }
}