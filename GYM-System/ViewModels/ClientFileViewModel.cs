using GYM_System.Models;

namespace GYM_System.ViewModels
{
    public class ClientFileViewModel
    {
        public Client? Client { get; set; }
        public IEnumerable<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public IEnumerable<ClientAssessment> ClientAssessments { get; set; } = new List<ClientAssessment>();
        public IEnumerable<ClientUpdate> ClientUpdates { get; set; } = new List<ClientUpdate>();
        public IEnumerable<DietPlan> DietPlans { get; set; } = new List<DietPlan>();
        public IEnumerable<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();

        // Constructor to initialize with a client and empty collections
        public ClientFileViewModel(Client client)
        {
            Client = client;
        }

        public ClientFileViewModel() { }
    }
}
