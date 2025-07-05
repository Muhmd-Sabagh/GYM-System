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
        public DateTime Timestamp { get; set; } // When the form was submitted

        [Required]
        [StringLength(50)]
        public string FormCode { get; set; } = string.Empty; // To link back to the client

        // Update Form Fields
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CurrentWeightKg { get; set; }

        [StringLength(500)]
        public string? PhysiqueChanges { get; set; } // Notes on physique changes

        [StringLength(500)]
        public string? DietAdjustmentsNotes { get; set; }

        [StringLength(500)]
        public string? ExerciseAdjustmentsNotes { get; set; }

        [StringLength(1000)]
        public string? AdditionalNotes { get; set; }

        [StringLength(500)]
        public string? UploadedImageUrl { get; set; } // URL for uploaded images (if any)
    }
}