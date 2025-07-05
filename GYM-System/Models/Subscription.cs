using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_System.Models
{
    public class Subscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; } // Foreign Key to Client

        [ForeignKey("ClientId")]
        public Client? Client { get; set; } // Navigation property

        [Required]
        [StringLength(100)]
        public string PackageType { get; set; } = string.Empty; // e.g., Diamond, Gold

        [Required]
        [DataType(DataType.Date)] // Ensures only date is picked by default in UI
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        [Range(1, 120)] // Duration in months, e.g., 1 to 120 months (10 years)
        public int DurationMonths { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // For currency
        [Range(0.01, 1000000.00)] // Example range for price
        public decimal Price { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "EGP"; // e.g., EGP, USD

        [StringLength(100)]
        public string? PaymentAccount { get; set; } // e.g., Bank Transfer, Cash

        public int RenewalCount { get; set; } = 0; // Tracks renewals for this subscription

        // Helper property to check if the subscription is currently active
        [NotMapped] // This property won't be mapped to the database
        public bool IsActive
        {
            get
            {
                return DateTime.Now >= StartDate && DateTime.Now < StartDate.AddMonths(DurationMonths);
            }
        }
    }
}
