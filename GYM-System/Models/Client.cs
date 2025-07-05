using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_System.Models
{
    public class Client
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Primary Key

        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(); // Automatically generated

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(255)]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public DateTime JoinDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string FormCode { get; set; } = Guid.NewGuid().ToString().Substring(0, 10).ToUpper(); // Unique for Google Forms

        // Navigation properties
        public ICollection<Subscription>? Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<ClientAssessment>? ClientAssessments { get; set; } = new List<ClientAssessment>();
        public ICollection<ClientUpdate>? ClientUpdates { get; set; } = new List<ClientUpdate>();

        // Properties for client status tracking (enums will be defined later)
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Inactive;
        public PlanStatus DietStatus { get; set; } = PlanStatus.NotStarted;
        public PlanStatus WorkoutStatus { get; set; } = PlanStatus.NotStarted;

        // Helper method to check if the client has an active subscription
        [NotMapped] // This property won't be mapped to the database
        public bool HasActiveSubscription
        {
            get
            {
                return Subscriptions != null && Subscriptions.Any(s => s.IsActive);
            }
        }
    }

    // Enum for Subscription Status
    public enum SubscriptionStatus
    {
        Active,
        Expired,
        Inactive // Default for new clients without an active subscription
    }

    // Enum for Plan Status (Diet/Workout)
    public enum PlanStatus
    {
        NotStarted,
        WaitingForPlan,
        OnPlan,
        NeedsUpdateForm
    }
}
