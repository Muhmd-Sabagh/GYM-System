using GYM_System.Data;
using GYM_System.Models;
using GYM_System.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GYM_System.Controllers
{
    public class DietPlanMakerController : Controller
    {
        readonly GymDbContext _context;

        public DietPlanMakerController(GymDbContext context)
        {
            _context = context;
        }

        // GET: DietPlanMaker/Index
        // Displays the main diet plan creation/editing interface
        public async Task<IActionResult> Index(int? id)
        {
            DietPlanViewModel viewModel;

            if (id.HasValue)
            {
                // Load an existing diet plan for editing
                var dietPlan = await _context.DietPlans
                                            .Include(dp => dp.Client) // Include client if linked
                                            .Include(dp => dp.Versions)
                                                .ThenInclude(dpv => dpv.Meals)
                                                    .ThenInclude(m => m.MealFoodItems)
                                                        .ThenInclude(mfi => mfi.FoodItem) // Eager load food items for calculations
                                            .FirstOrDefaultAsync(dp => dp.Id == id);

                if (dietPlan == null)
                {
                    TempData["ErrorMessage"] = "Diet Plan not found.";
                    return RedirectToAction(nameof(Index)); // Redirect to new plan if not found
                }
                viewModel = new DietPlanViewModel(dietPlan);
                // Populate calculated macros after loading
                CalculateMacrosForViewModel(viewModel);
            }
            else
            {
                // Create a new, empty diet plan view model
                viewModel = new DietPlanViewModel();
            }

            ViewBag.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync(); // For client dropdown
            ViewBag.FoodItems = await _context.FoodItems.OrderBy(fi => fi.Name).ToListAsync(); // For food item search

            return View(viewModel);
        }

        // POST: DietPlanMaker/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(DietPlanViewModel viewModel)
        {
            // Manual validation for nested collections
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Validation errors occurred. Please check your inputs.";
                ViewBag.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
                ViewBag.FoodItems = await _context.FoodItems.OrderBy(fi => fi.Name).ToListAsync();
                CalculateMacrosForViewModel(viewModel); // Recalculate for display
                return View("Index", viewModel);
            }

            // Map ViewModel to Entity Models
            DietPlan dietPlan;
            if (viewModel.Id == 0) // New plan
            {
                dietPlan = new DietPlan
                {
                    CreatedDate = DateTime.Now
                };
                _context.Add(dietPlan);
            }
            else // Existing plan
            {
                dietPlan = await _context.DietPlans
                                        .Include(dp => dp.Versions)
                                            .ThenInclude(dpv => dpv.Meals)
                                                .ThenInclude(m => m.MealFoodItems)
                                        .FirstOrDefaultAsync(dp => dp.Id == viewModel.Id);

                if (dietPlan == null)
                {
                    TempData["ErrorMessage"] = "Diet Plan not found for update.";
                    return RedirectToAction(nameof(Index));
                }
                // Clear existing relationships to rebuild them from the view model
                _context.MealFoodItems.RemoveRange(dietPlan.Versions.SelectMany(v => v.Meals).SelectMany(m => m.MealFoodItems));
                _context.Meals.RemoveRange(dietPlan.Versions.SelectMany(v => v.Meals));
                _context.DietPlanVersions.RemoveRange(dietPlan.Versions);
            }

            dietPlan.PlanName = viewModel.PlanName;
            dietPlan.ClientId = viewModel.ClientId;
            dietPlan.GeneralNotes = viewModel.GeneralNotes;

            foreach (var versionVm in viewModel.Versions)
            {
                var version = new DietPlanVersion
                {
                    VersionName = versionVm.VersionName,
                    IsActiveForPdf = versionVm.IsActiveForPdf,
                    VersionNotes = versionVm.VersionNotes
                };
                dietPlan.Versions?.Add(version);

                foreach (var mealVm in versionVm.Meals)
                {
                    var meal = new Meal
                    {
                        MealName = mealVm.MealName,
                        MealNotes = mealVm.MealNotes
                    };
                    version.Meals?.Add(meal);

                    // Eager load food items for calculation correctness during save
                    var foodItems = await _context.FoodItems.ToListAsync();
                    foreach (var mfiVm in mealVm.MealFoodItems)
                    {
                        // Find the corresponding FoodItem from the database
                        var foodItem = foodItems.FirstOrDefault(fi => fi.Id == mfiVm.FoodItemId);
                        if (foodItem == null)
                        {
                            // This should ideally be caught by client-side validation
                            // or provide more specific error feedback.
                            TempData["ErrorMessage"] = $"Food item with ID {mfiVm.FoodItemId} not found. Please re-select.";
                            ViewBag.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
                            ViewBag.FoodItems = foodItems;
                            CalculateMacrosForViewModel(viewModel);
                            return View("Index", viewModel);
                        }

                        var mealFoodItem = new MealFoodItem
                        {
                            FoodItemId = mfiVm.FoodItemId,
                            Quantity = mfiVm.Quantity
                        };
                        meal.MealFoodItems?.Add(mealFoodItem);
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Diet Plan '{viewModel.PlanName}' saved successfully!";
            return RedirectToAction(nameof(Index), new { id = dietPlan.Id }); // Redirect to the saved plan
        }

        // POST: DietPlanMaker/GeneratePdf (Placeholder)
        // This will be implemented in the next iteration.
        [HttpPost]
        public async Task<IActionResult> GeneratePdf(int id)
        {
            // Fetch the diet plan from DB, populate ViewModel, then generate PDF
            // using QuestPDF.
            TempData["InfoMessage"] = "PDF generation coming soon!";
            return RedirectToAction(nameof(Index), new { id = id });
        }


        // Helper to calculate total macros for a MealViewModel
        private void CalculateMacrosForMealViewModel(MealViewModel mealVm, List<FoodItem> allFoodItems)
        {
            mealVm.TotalCalories = 0;
            mealVm.TotalProtein = 0;
            mealVm.TotalCarbs = 0;
            mealVm.TotalFat = 0;

            if (mealVm.MealFoodItems != null)
            {
                foreach (var mfiVm in mealVm.MealFoodItems)
                {
                    var foodItem = allFoodItems.FirstOrDefault(fi => fi.Id == mfiVm.FoodItemId);
                    if (foodItem != null)
                    {
                        decimal factor = mfiVm.Quantity / 100m; // Calories/macros are per 100 units
                        mfiVm.Calories = foodItem.CaloriesPer100Units * factor;
                        mfiVm.Protein = foodItem.ProteinPer100Units * factor;
                        mfiVm.Carbs = foodItem.CarbsPer100Units * factor;
                        mfiVm.Fat = foodItem.FatPer100Units * factor;

                        mealVm.TotalCalories += mfiVm.Calories;
                        mealVm.TotalProtein += mfiVm.Protein;
                        mealVm.TotalCarbs += mfiVm.Carbs;
                        mealVm.TotalFat += mfiVm.Fat;
                    }
                }
            }
        }

        // Helper to calculate total macros for a DietPlanVersionViewModel
        private void CalculateMacrosForVersionViewModel(DietPlanVersionViewModel versionVm, List<FoodItem> allFoodItems)
        {
            versionVm.Meals?.ForEach(m => CalculateMacrosForMealViewModel(m, allFoodItems));

            // Sum totals from all meals in this version
            if (versionVm.Meals != null)
            {
                foreach (var meal in versionVm.Meals)
                {
                    // These are NotMapped on MealViewModel, so need to sum them up.
                    // For now, we will add them here, but the values will be stored in MealFoodItemViewModel.
                    // This method primarily populates MealFoodItemViewModel and MealViewModel's totals for UI display.
                }
            }
        }
        // Master method to calculate all macros for the entire DietPlanViewModel
        private void CalculateMacrosForViewModel(DietPlanViewModel viewModel)
        {
            var allFoodItems = _context.FoodItems.AsNoTracking().ToList(); // Get all food items once for efficient lookup

            if (viewModel.Versions != null)
            {
                foreach (var versionVm in viewModel.Versions)
                {
                    if (versionVm.Meals != null)
                    {
                        foreach (var mealVm in versionVm.Meals)
                        {
                            CalculateMacrosForMealViewModel(mealVm, allFoodItems);
                            // Sum up totals for the version from its meals.
                            // The properties TotalCalories, TotalProtein, etc., on DietPlanVersionViewModel are not directly used in the current version of the ViewModel.
                            // This would be the place to calculate them if they were part of the ViewModel.
                        }
                    }
                }
            }
        }
    }
}