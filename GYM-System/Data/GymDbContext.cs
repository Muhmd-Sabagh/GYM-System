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

            // Client and Assessment/Update relationships
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

            // Diet Plan relationships
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
                .WithMany() // No navigation property on FoodItem back to MealFoodItems directly needed for this one-to-many
                .HasForeignKey(mfi => mfi.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting food item if it's used in a plan
        }
    }
}