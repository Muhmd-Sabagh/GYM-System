using GYM_System.Models;
using Microsoft.EntityFrameworkCore;

namespace GYM_System.Data
{
    public class GymDbContext : DbContext
    {
        public GymDbContext(DbContextOptions<GymDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; } = default!;
        public DbSet<Subscription> Subscriptions { get; set; } = default!;
        public DbSet<FoodItem> FoodItems { get; set; } = default!;
        public DbSet<Exercise> Exercises { get; set; } = default!;
        public DbSet<ClientAssessment> ClientAssessments { get; set; } = default!;
        public DbSet<ClientUpdate> ClientUpdates { get; set; } = default!;
        public DbSet<DietPlan> DietPlans { get; set; } = default!;
        public DbSet<DietPlanVersion> DietPlanVersions { get; set; } = default!;
        public DbSet<Meal> Meals { get; set; } = default!;
        public DbSet<MealFoodItem> MealFoodItems { get; set; } = default!;
        public DbSet<WorkoutPlan> WorkoutPlans { get; set; } = default!;
        public DbSet<WorkoutDay> WorkoutDays { get; set; } = default!;
        public DbSet<WorkoutExercise> WorkoutExercises { get; set; } = default!;
        public DbSet<Package> Packages { get; set; } = default!;
        public DbSet<Currency> Currencies { get; set; } = default!;
        public DbSet<PaymentAccount> PaymentAccounts { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Client and Subscription relationships
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.ClientCode)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.FormCode)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .HasMany(c => c.Subscriptions)
                .WithOne(s => s.Client)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.PackageType)
                .WithMany()
                .HasForeignKey(s => s.PackageTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Currency)
                .WithMany()
                .HasForeignKey(s => s.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.PaymentAccount)
                .WithMany()
                .HasForeignKey(s => s.PaymentAccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Client>()
                .HasMany(c => c.ClientAssessments)
                .WithOne(ca => ca.Client)
                .HasForeignKey(ca => ca.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Client>()
                .HasMany(c => c.ClientUpdates)
                .WithOne(cu => cu.Client)
                .HasForeignKey(cu => cu.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- ADD THESE NEW RELATIONSHIP DEFINITIONS ---
            modelBuilder.Entity<Client>()
                .HasMany(c => c.DietPlans)
                .WithOne(dp => dp.Client) // Assuming DietPlan has a Client navigation property
                .HasForeignKey(dp => dp.ClientId) // Assuming DietPlan has a ClientId foreign key
                .OnDelete(DeleteBehavior.SetNull); // Or .Cascade, depending on desired behavior. SetNull is safer if you want to keep plans after client deletion, but requires ClientId to be nullable in DietPlan. Let's use Cascade for now, assuming plans are tied to the client.

            modelBuilder.Entity<Client>()
                .HasMany(c => c.WorkoutPlans)
                .WithOne(wp => wp.Client) // Assuming WorkoutPlan has a Client navigation property
                .HasForeignKey(wp => wp.ClientId) // Assuming WorkoutPlan has a ClientId foreign key
                .OnDelete(DeleteBehavior.SetNull); // Or .Cascade. Using SetNull for consistency with DietPlan.

            // Diet Plan relationships (existing)
            modelBuilder.Entity<DietPlan>()
                .HasMany(dp => dp.Versions)
                .WithOne(dpv => dpv.DietPlan)
                .HasForeignKey(dpv => dpv.DietPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DietPlanVersion>()
                .HasMany(dpv => dpv.Meals)
                .WithOne(m => m.DietPlanVersion)
                .HasForeignKey(m => m.DietPlanVersionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Meal>()
                .HasMany(m => m.MealFoodItems)
                .WithOne(mfi => mfi.Meal)
                .HasForeignKey(mfi => mfi.MealId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MealFoodItem>()
                .HasOne(mfi => mfi.FoodItem)
                .WithMany()
                .HasForeignKey(mfi => mfi.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Workout Plan relationships (existing)
            modelBuilder.Entity<WorkoutPlan>()
                .HasMany(wp => wp.WorkoutDays)
                .WithOne(wd => wd.WorkoutPlan)
                .HasForeignKey(wd => wd.WorkoutPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkoutDay>()
                .HasMany(wd => wd.WorkoutExercises)
                .WithOne(we => we.WorkoutDay)
                .HasForeignKey(we => we.WorkoutDayId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkoutExercise>()
                .HasOne(we => we.Exercise)
                .WithMany()
                .HasForeignKey(we => we.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}