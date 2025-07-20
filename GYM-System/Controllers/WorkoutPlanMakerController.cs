using GYM_System.Data;
using GYM_System.Models;
using GYM_System.Services;
using GYM_System.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SuperSheets.Controllers
{
    public class WorkoutPlanMakerController : Controller
    {
        private readonly GymDbContext _context;
        private readonly PdfService _pdfService;

        public WorkoutPlanMakerController(GymDbContext context, PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        // GET: WorkoutPlanMaker/Index
        // Displays the main workout plan creation/editing interface
        public async Task<IActionResult> Index(int? id)
        {
            WorkoutPlanViewModel viewModel;

            if (id.HasValue)
            {
                // Load an existing workout plan for editing
                var workoutPlan = await _context.WorkoutPlans
                                                .Include(wp => wp.Client) // Include client if linked
                                                .Include(wp => wp.WorkoutDays)
                                                    .ThenInclude(wd => wd.WorkoutExercises)
                                                        .ThenInclude(we => we.Exercise) // Eager load exercises
                                                .FirstOrDefaultAsync(wp => wp.Id == id);

                if (workoutPlan == null)
                {
                    TempData["ErrorMessage"] = "Workout Plan not found.";
                    return RedirectToAction(nameof(Index)); // Redirect to new plan if not found
                }
                viewModel = new WorkoutPlanViewModel(workoutPlan);
            }
            else
            {
                // Create a new, empty workout plan view model
                viewModel = new WorkoutPlanViewModel();
            }

            ViewBag.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync(); // For client dropdown
            ViewBag.Exercises = await _context.Exercises.OrderBy(e => e.Name).ToListAsync(); // For exercise search

            return View(viewModel);
        }

        // POST: WorkoutPlanMaker/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(WorkoutPlanViewModel viewModel)
        {
            // Manually remove validation errors for properties that are not directly bound
            ModelState.Remove("Client");
            foreach (var day in viewModel.WorkoutDays)
            {
                ModelState.Remove($"WorkoutDays[{viewModel.WorkoutDays.IndexOf(day)}].WorkoutExercises");
                foreach (var exercise in day.WorkoutExercises)
                {
                    ModelState.Remove($"WorkoutDays[{viewModel.WorkoutDays.IndexOf(day)}].WorkoutExercises[{day.WorkoutExercises.IndexOf(exercise)}].Exercise");
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Validation errors occurred. Please check your inputs.";
                ViewBag.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
                ViewBag.Exercises = await _context.Exercises.OrderBy(e => e.Name).ToListAsync();
                return View("Index", viewModel);
            }

            // Map ViewModel to Entity Models
            WorkoutPlan workoutPlan;
            if (viewModel.Id == 0) // New plan
            {
                workoutPlan = new WorkoutPlan
                {
                    CreatedDate = DateTime.Now
                };
                _context.Add(workoutPlan);
            }
            else // Existing plan
            {
                workoutPlan = await _context.WorkoutPlans
                                            .Include(wp => wp.WorkoutDays)
                                                .ThenInclude(wd => wd.WorkoutExercises)
                                            .FirstOrDefaultAsync(wp => wp.Id == viewModel.Id);

                if (workoutPlan == null)
                {
                    TempData["ErrorMessage"] = "Workout Plan not found for update.";
                    return RedirectToAction(nameof(Index));
                }
                // Clear existing relationships to rebuild them from the view model
                _context.WorkoutExercises.RemoveRange(workoutPlan.WorkoutDays.SelectMany(wd => wd.WorkoutExercises));
                _context.WorkoutDays.RemoveRange(workoutPlan.WorkoutDays);
            }

            workoutPlan.PlanName = viewModel.PlanName;
            workoutPlan.ClientId = viewModel.ClientId;
            workoutPlan.GeneralNotes = viewModel.GeneralNotes;

            foreach (var dayVm in viewModel.WorkoutDays)
            {
                var workoutDay = new WorkoutDay
                {
                    DayName = dayVm.DayName,
                    Subtitle = dayVm.Subtitle,
                    DayNotes = dayVm.DayNotes
                };
                workoutPlan.WorkoutDays?.Add(workoutDay);

                foreach (var weVm in dayVm.WorkoutExercises)
                {
                    // Ensure the exercise exists
                    var exercise = await _context.Exercises.FindAsync(weVm.ExerciseId);
                    if (exercise == null)
                    {
                        TempData["ErrorMessage"] = $"Exercise with ID {weVm.ExerciseId} not found. Please re-select.";
                        ViewBag.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
                        ViewBag.Exercises = await _context.Exercises.OrderBy(e => e.Name).ToListAsync();
                        return View("Index", viewModel);
                    }

                    var workoutExercise = new WorkoutExercise
                    {
                        ExerciseId = weVm.ExerciseId,
                        Sets = weVm.Sets,
                        Reps = weVm.Reps,
                        Rest = weVm.Rest,
                        Tempo = weVm.Tempo,
                        RpeRir = weVm.RpeRir,
                        ExerciseNotes = weVm.ExerciseNotes
                    };
                    workoutDay.WorkoutExercises?.Add(workoutExercise);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Workout Plan '{viewModel.PlanName}' saved successfully!";
            return RedirectToAction(nameof(Index), new { id = workoutPlan.Id }); // Redirect to the saved plan
        }

        // POST: WorkoutPlanMaker/GeneratePdf
        [HttpPost]
        public async Task<IActionResult> GeneratePdf(int id)
        {
            var workoutPlan = await _context.WorkoutPlans
                                            .Include(wp => wp.Client)
                                            .Include(wp => wp.WorkoutDays)
                                                .ThenInclude(wd => wd.WorkoutExercises)
                                                    .ThenInclude(we => we.Exercise)
                                            .FirstOrDefaultAsync(wp => wp.Id == id);

            if (workoutPlan == null)
            {
                TempData["ErrorMessage"] = "Workout Plan not found for PDF generation.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new WorkoutPlanViewModel(workoutPlan);

            try
            {
                byte[] pdfBytes = _pdfService.GenerateWorkoutPlanPdf(viewModel); // Call new PDF method
                string filePath = _pdfService.SaveWorkoutPlanPdf(pdfBytes, viewModel.PlanName); // New save method

                TempData["SuccessMessage"] = $"PDF for '{viewModel.PlanName}' generated and saved to '{filePath}'!";
                return File(pdfBytes, "application/pdf", $"{viewModel.PlanName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating PDF: {ex.Message}";
                Console.WriteLine($"PDF Generation Error: {ex}");
                return RedirectToAction(nameof(Index), new { id = id });
            }
        }

        // GET: WorkoutPlanMaker/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workoutPlan = await _context.WorkoutPlans
                .Include(wp => wp.Client)
                .Include(wp => wp.WorkoutDays)
                    .ThenInclude(wd => wd.WorkoutExercises)
                        .ThenInclude(we => we.Exercise)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (workoutPlan == null)
            {
                return NotFound();
            }

            // For display on the delete confirmation page
            var viewModel = new WorkoutPlanViewModel(workoutPlan);

            return View(viewModel);
        }

        // POST: WorkoutPlanMaker/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workoutPlan = await _context.WorkoutPlans
                .Include(wp => wp.WorkoutDays)
                    .ThenInclude(wd => wd.WorkoutExercises)
                .FirstOrDefaultAsync(wp => wp.Id == id);

            if (workoutPlan != null)
            {
                // Manually remove child entities to avoid issues with cascade delete if not configured
                _context.WorkoutExercises.RemoveRange(workoutPlan.WorkoutDays.SelectMany(wd => wd.WorkoutExercises));
                _context.WorkoutDays.RemoveRange(workoutPlan.WorkoutDays);
                _context.WorkoutPlans.Remove(workoutPlan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Workout Plan deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Workout Plan not found for deletion.";
            }
            return RedirectToAction("Index", "SavedPlans"); // Redirect to Saved Plans list
        }
    }
}