using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_System.Models
{
    public class PaymentAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Account Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Details")]
        public string? Details { get; set; } // e.g., Bank Account Number, Phone Number

        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}
