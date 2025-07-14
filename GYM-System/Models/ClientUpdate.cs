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
        [Display(Name = "Update Date")] // Renamed from Timestamp for clarity
        public DateTime Timestamp { get; set; } // When the form was submitted

        [Required]
        [StringLength(50)]
        public string FormCode { get; set; } = string.Empty; // To link back to the client

        // --- New Update Form Fields based on "Assessment Questions.md" ---

        // Body Assessment Update (from original update section questions 8-15)
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

        // Workout Assessment Update (from original update section question 16-17)
        [Display(Name = "Workout Commitment Level")]
        [StringLength(100)]
        public string? WorkoutCommitmentLevel { get; set; } // (اختياري) - غير ملتزم - ملتزم الى حد ما - ملتزم بالكامل

        [Display(Name = "Notes")]
        [StringLength(1000)]
        public string? Notes { get; set; } // (اختياري) - General notes for the update
    }
}