using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_System.Models
{
    public class Currency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Currency Code")]
        public string Code { get; set; } = string.Empty; // e.g., EGP, USD

        [Required]
        [StringLength(100)]
        [Display(Name = "Currency Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(5)]
        [Display(Name = "Symbol")]
        public string? Symbol { get; set; } // e.g., £, $

        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}
