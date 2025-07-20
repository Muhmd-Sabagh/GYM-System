using GYM_System.Data;
using GYM_System.Models;
using GYM_System.Services;
using GYM_System.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SuperSheets.Controllers
{
    public class DietPlanMakerController : Controller
    {
        private readonly GymDbContext _context;
        private readonly PdfService _pdfService;

        public DietPlanMakerController(GymDbContext context, PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
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
            // Manually remove validation errors for properties that are not directly bound
            // or are calculated/navigation properties to prevent issues with ModelState.IsValid
            ModelState.Remove("Client");
            foreach (var version in viewModel.Versions)
            {
                ModelState.Remove($"Versions[{viewModel.Versions.IndexOf(version)}].Meals");
                foreach (var meal in version.Meals)
                {
                    ModelState.Remove($"Versions[{viewModel.Versions.IndexOf(version)}].Meals[{version.Meals.IndexOf(meal)}].MealFoodItems");
                    ModelState.Remove($"Versions[{viewModel.Versions.IndexOf(version)}].Meals[{version.Meals.IndexOf(meal)}].TotalCalories");
                    ModelState.Remove($"Versions[{viewModel.Versions.IndexOf(version)}].Meals[{version.Meals.IndexOf(meal)}].TotalProtein");
                    ModelState.Remove($"Versions[{viewModel.Versions.IndexOf(version)}].Meals[{version.Meals.IndexOf(meal)}].TotalCarbs");
                    ModelState.Remove($"Versions[{viewModel.Versions.IndexOf(version)}].Meals[{version.Meals.IndexOf(meal)}].TotalFat");

                    foreach (var mfi in meal.MealFoodItems)
                    {
                        ModelState.Remove($"Versions[{viewModel.Versions.IndexOf(version)}].Meals[{version.Meals.IndexOf(meal)}].MealFoodItems[{meal.MealFoodItems.IndexOf(mfi)}].FoodItem");
                        ModelState.Remove($"Versions[{viewModel.Versions.IndexOf(version)}].Meals[{version.Meals.IndexOf(meal)}].MealFoodItems[{meal.MealFoodItems.IndexOf(mfi)}].Calories");
                        ModelState.Remove($"Versions[{viewModel.Versions.IndexOf(version)}].Meals[{version.Meals.IndexOf(meal)}].MealFoodItems[{meal.MealFoodItems.IndexOf(mfi)}].Protein");
                        ModelState.Remove($"Versions[{viewModel.Versions.IndexOf(version)}].Meals[{version.Meals.IndexOf(meal)}].MealFoodItems[{meal.MealFoodItems.IndexOf(mfi)}].Carbs");
                        ModelState.Remove($"Versions[{viewModel.Versions.IndexOf(version)}].Meals[{version.Meals.IndexOf(meal)}].MealFoodItems[{meal.MealFoodItems.IndexOf(mfi)}].Fat");
                    }
                }
            }


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

        // POST: DietPlanMaker/GeneratePdf
        [HttpPost]
        public async Task<IActionResult> GeneratePdf(int id)
        {
            var dietPlan = await _context.DietPlans
                                        .Include(dp => dp.Client)
                                        .Include(dp => dp.Versions)
                                            .ThenInclude(dpv => dpv.Meals)
                                                .ThenInclude(m => m.MealFoodItems)
                                                    .ThenInclude(mfi => mfi.FoodItem)
                                        .FirstOrDefaultAsync(dp => dp.Id == id);

            if (dietPlan == null)
            {
                TempData["ErrorMessage"] = "Diet Plan not found for PDF generation.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new DietPlanViewModel(dietPlan);
            CalculateMacrosForViewModel(viewModel); // Populate calculated macros for PDF generation

            try
            {
                byte[] pdfBytes = _pdfService.GenerateDietPlanPdf(viewModel);
                string filePath = _pdfService.SaveDietPlanPdf(pdfBytes, viewModel.PlanName);

                TempData["SuccessMessage"] = $"PDF for '{viewModel.PlanName}' generated and saved to '{filePath}'!";
                return File(pdfBytes, "application/pdf", $"{viewModel.PlanName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating PDF: {ex.Message}";
                // Log the exception for debugging
                Console.WriteLine($"PDF Generation Error: {ex}");
                return RedirectToAction(nameof(Index), new { id = id });
            }
        }

        // GET: DietPlanMaker/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dietPlan = await _context.DietPlans
                .Include(dp => dp.Client)
                .Include(dp => dp.Versions)
                    .ThenInclude(dpv => dpv.Meals)
                        .ThenInclude(m => m.MealFoodItems)
                            .ThenInclude(mfi => mfi.FoodItem)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dietPlan == null)
            {
                return NotFound();
            }

            // Populate calculated macros for display on the delete confirmation page
            var viewModel = new DietPlanViewModel(dietPlan);
            CalculateMacrosForViewModel(viewModel);

            return View(viewModel);
        }

        // POST: DietPlanMaker/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dietPlan = await _context.DietPlans
                .Include(dp => dp.Versions)
                    .ThenInclude(dpv => dpv.Meals)
                        .ThenInclude(m => m.MealFoodItems)
                .FirstOrDefaultAsync(dp => dp.Id == id);

            if (dietPlan != null)
            {
                // Manually remove child entities to avoid issues with cascade delete if not configured
                _context.MealFoodItems.RemoveRange(dietPlan.Versions.SelectMany(v => v.Meals).SelectMany(m => m.MealFoodItems));
                _context.Meals.RemoveRange(dietPlan.Versions.SelectMany(v => v.Meals));
                _context.DietPlanVersions.RemoveRange(dietPlan.Versions);
                _context.DietPlans.Remove(dietPlan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Diet Plan deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Diet Plan not found for deletion.";
            }
            return RedirectToAction("Index", "SavedPlans"); // Redirect to Saved Plans list
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

        // Master method to calculate all macros for the entire DietPlanViewModel
        private void CalculateMacrosForViewModel(DietPlanViewModel viewModel)
        {
            // Fetch all food items once to avoid multiple DB calls
            var allFoodItems = _context.FoodItems.AsNoTracking().ToList();

            if (viewModel.Versions != null)
            {
                foreach (var versionVm in viewModel.Versions)
                {
                    if (versionVm.Meals != null)
                    {
                        foreach (var mealVm in versionVm.Meals)
                        {
                            CalculateMacrosForMealViewModel(mealVm, allFoodItems);
                        }
                    }
                }
            }
        }
    }
}