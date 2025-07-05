using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_System.Models
{
    public class FoodItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? ImagePath { get; set; } // Path to locally stored image

        [Required]
        [StringLength(50)]
        public string Unit { get; set; } = "gram"; // e.g., gram, slice, piece, large egg

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, 15000)]
        public decimal CaloriesPer100Units { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, 5000)]
        public decimal ProteinPer100Units { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, 5000)]
        public decimal CarbsPer100Units { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, 1000)]
        public decimal FatPer100Units { get; set; }
    }
}
