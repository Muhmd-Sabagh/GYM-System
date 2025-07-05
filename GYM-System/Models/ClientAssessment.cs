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
        public DateTime Timestamp { get; set; } // When the form was submitted

        [Required]
        [StringLength(50)]
        public string FormCode { get; set; } = string.Empty; // To link back to the client

        // Initial Assessment Fields (from "First Plans" form)
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? HeightCm { get; set; } // e.g., in centimeters

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? WeightKg { get; set; } // e.g., in kilograms

        public int? Age { get; set; }

        [StringLength(50)]
        public string? Gender { get; set; } // e.g., Male, Female

        [StringLength(100)]
        public string? ActivityLevel { get; set; } // e.g., Sedentary, Moderate, Active

        [StringLength(500)]
        public string? DietaryPreferences { get; set; } // e.g., Vegetarian, Vegan, Allergies

        [StringLength(500)]
        public string? FitnessGoals { get; set; } // e.g., Lose Fat, Gain Muscle, Strength

        [StringLength(1000)]
        public string? OtherNotes { get; set; }

        [StringLength(500)]
        public string? UploadedImageUrl { get; set; } // URL for uploaded images (if any, from Google Drive)
    }
}