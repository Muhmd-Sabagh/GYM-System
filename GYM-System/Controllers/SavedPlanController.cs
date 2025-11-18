using GYM_System.Data;
using GYM_System.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GYM_System.Controllers
{
    public class SavedPlansController : Controller
    {
        private readonly GymDbContext _context;

        public SavedPlansController(GymDbContext context)
        {
            _context = context;
        }

        // GET: SavedPlans
        public async Task<IActionResult> Index(
            string? searchString,
            string? clientCode,
            DateTime? startDate,
            DateTime? endDate,
            PlanType? planType,
            decimal? minCalories,
            decimal? maxCalories,
            decimal? minProtein,
            decimal? maxProtein,
            decimal? minCarbs,
            decimal? maxCarbs,
            decimal? minFat,
            decimal? maxFat,
            int? minDays,
            int? maxDays)
        {
            var dietPlansQuery = _context.DietPlans
                                         .Include(dp => dp.Client)
                                         .Include(dp => dp.Versions)
                                            .ThenInclude(dpv => dpv.Meals)
                                                .ThenInclude(m => m.MealFoodItems)
                                                    .ThenInclude(mfi => mfi.FoodItem)
                                         .AsQueryable();

            var workoutPlansQuery = _context.WorkoutPlans
                                            .Include(wp => wp.Client)
                                            .Include(wp => wp.WorkoutDays)
                                            .AsQueryable();

            // Fetch all food items once for macro calculation
            var allFoodItems = await _context.FoodItems.AsNoTracking().ToListAsync();

            var allPlans = new List<SavedPlanSummaryViewModel>();

            // Process Diet Plans
            foreach (var dp in await dietPlansQuery.ToListAsync())
            {
                // Calculate total macros for each diet plan for filtering
                decimal totalCalories = 0;
                decimal totalProtein = 0;
                decimal totalCarbs = 0;
                decimal totalFat = 0;

                foreach (var version in dp.Versions)
                {
                    foreach (var meal in version.Meals)
                    {
                        foreach (var mfi in meal.MealFoodItems)
                        {
                            var foodItem = allFoodItems.FirstOrDefault(fi => fi.Id == mfi.FoodItemId);
                            if (foodItem != null)
                            {
                                decimal factor = mfi.Quantity / 100m;
                                totalCalories += foodItem.CaloriesPer100Units * factor;
                                totalProtein += foodItem.ProteinPer100Units * factor;
                                totalCarbs += foodItem.CarbsPer100Units * factor;
                                totalFat += foodItem.FatPer100Units * factor;
                            }
                        }
                    }
                }

                allPlans.Add(new SavedPlanSummaryViewModel
                {
                    Id = dp.Id,
                    PlanName = dp.PlanName,
                    ClientId = dp.ClientId,
                    ClientName = dp.Client?.Name,
                    CreatedDate = dp.CreatedDate,
                    Type = PlanType.Diet,
                    TotalCalories = totalCalories,
                    TotalProtein = totalProtein,
                    TotalCarbs = totalCarbs,
                    TotalFat = totalFat
                });
            }

            // Process Workout Plans
            foreach (var wp in await workoutPlansQuery.ToListAsync())
            {
                allPlans.Add(new SavedPlanSummaryViewModel
                {
                    Id = wp.Id,
                    PlanName = wp.PlanName,
                    ClientId = wp.ClientId,
                    ClientName = wp.Client?.Name,
                    CreatedDate = wp.CreatedDate,
                    Type = PlanType.Workout,
                    NumberOfDays = wp.WorkoutDays?.Count ?? 0
                });
            }

            // Apply filters
            var filteredPlans = allPlans.AsEnumerable(); // Switch to LINQ to Objects for filtering after fetching

            if (!string.IsNullOrEmpty(searchString))
            {
                filteredPlans = filteredPlans.Where(p => p.PlanName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                                                        (p.ClientName != null && p.ClientName.Contains(searchString, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrEmpty(clientCode))
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientCode == clientCode);
                if (client != null)
                {
                    filteredPlans = filteredPlans.Where(p => p.ClientName == client.Name); // Filter by client name
                }
                else
                {
                    // If client code is provided but no matching client, return empty results
                    filteredPlans = filteredPlans.Where(p => false);
                }
            }

            if (startDate.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.CreatedDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.CreatedDate.Date <= endDate.Value.Date);
            }

            if (planType.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.Type == planType.Value);
            }

            // Diet-specific filters
            if (minCalories.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.Type == PlanType.Diet && p.TotalCalories >= minCalories.Value);
            }
            if (maxCalories.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.Type == PlanType.Diet && p.TotalCalories <= maxCalories.Value);
            }
            if (minProtein.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.Type == PlanType.Diet && p.TotalProtein >= minProtein.Value);
            }
            if (maxProtein.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.Type == PlanType.Diet && p.TotalProtein <= maxProtein.Value);
            }
            if (minCarbs.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.Type == PlanType.Diet && p.TotalCarbs >= minCarbs.Value);
            }
            if (maxCarbs.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.Type == PlanType.Diet && p.TotalCarbs <= maxCarbs.Value);
            }
            if (minFat.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.Type == PlanType.Diet && p.TotalFat >= minFat.Value);
            }
            if (maxFat.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.Type == PlanType.Diet && p.TotalFat <= maxFat.Value);
            }

            // Workout-specific filters
            if (minDays.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.Type == PlanType.Workout && p.NumberOfDays >= minDays.Value);
            }
            if (maxDays.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.Type == PlanType.Workout && p.NumberOfDays <= maxDays.Value);
            }

            // Order by creation date descending by default
            filteredPlans = filteredPlans.OrderByDescending(p => p.CreatedDate);

            ViewBag.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync(); // For client dropdown in filter

            return View(filteredPlans.ToList());
        }
    }
}