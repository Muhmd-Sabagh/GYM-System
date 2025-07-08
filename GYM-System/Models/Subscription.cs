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

        [Required(ErrorMessage = "Package Type is required.")]
        [Display(Name = "Package Type")]
        public int PackageTypeId { get; set; } // Foreign Key to Package
        [ForeignKey("PackageTypeId")]
        public Package? PackageType { get; set; } // Navigation property

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        [Range(1, 120)] // Duration in months, e.g., 1 to 120 months (10 years)
        public int DurationMonths { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // For currency
        [Range(0.01, 1000000.00)] // Example range for price
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [Display(Name = "Currency")]
        public int CurrencyId { get; set; } // Foreign Key to Currency
        [ForeignKey("CurrencyId")]
        public Currency? Currency { get; set; } // Navigation property

        [Display(Name = "Payment Account")]
        public int? PaymentAccountId { get; set; } // Foreign Key to PaymentAccount (nullable)
        [ForeignKey("PaymentAccountId")]
        public PaymentAccount? PaymentAccount { get; set; } // Navigation property

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
