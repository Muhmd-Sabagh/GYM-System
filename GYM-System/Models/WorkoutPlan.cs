using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_System.Models
{
    public class WorkoutPlan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string PlanName { get; set; } = string.Empty; // Internal name for the plan

        public int? ClientId { get; set; } // Optional: link to a client if created for a specific one
        [ForeignKey("ClientId")]
        public Client? Client { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property for workout days
        public ICollection<WorkoutDay>? WorkoutDays { get; set; } = new List<WorkoutDay>();

        // Optional general notes for the entire plan
        [StringLength(1000)]
        public string? GeneralNotes { get; set; }
    }
}