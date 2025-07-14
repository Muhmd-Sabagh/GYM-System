using GYM_System.Data;
using GYM_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GYM_System.Controllers
{
    public class ClientAssessmentsController : Controller
    {
        private readonly GymDbContext _context;

        public ClientAssessmentsController(GymDbContext context)
        {
            _context = context;
        }

        // GET: ClientAssessments
        // This Index will show all assessments, but typically you'd navigate from ClientFile
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ClientAssessments.Include(c => c.Client).OrderByDescending(c => c.Timestamp);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ClientAssessments/Details/5 (Optional, if you want a dedicated details page)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clientAssessment = await _context.ClientAssessments
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (clientAssessment == null)
            {
                return NotFound();
            }

            return View(clientAssessment);
        }

        // GET: ClientAssessments/Create
        // Can be accessed directly or with a clientId pre-filled
        public async Task<IActionResult> Create(int? clientId)
        {
            ViewBag.Clients = new SelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", clientId);

            // Pre-fill FormCode if client is selected
            if (clientId.HasValue)
            {
                var client = await _context.Clients.FindAsync(clientId.Value);
                if (client != null)
                {
                    ViewBag.DefaultFormCode = client.FormCode;
                }
            }

            return View();
        }

        // POST: ClientAssessments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(
            "ClientId,Timestamp,FormCode,Country,Religion,ServiceGoal,WeightKg,HeightCm,Gender,DateOfBirth,JobProfession," +
            "NeckCircumferenceCm,WaistCircumferenceCm,HipCircumferenceCm,ArmCircumferenceCm,ThighCircumferenceCm," +
            "FrontBodyPhotoPath,SideBodyPhotoPath,BackBodyPhotoPath," +
            "HasHealthProblems,HealthProblemsDetails,HasRecentTests,RecentTestsDetails,IsTakingMedicationsSupplements,MedicationsSupplementsDetails," +
            "HasMedicationAllergies,MedicationAllergiesDetails,HasChronicHereditaryDiseases,ChronicHereditaryDiseasesDetails," +
            "HasPastSurgeries,PastSurgeriesDetails,IsPregnantOrPlanning,IsTakingVitaminsMinerals,VitaminsMineralsDetails,OtherMedicalNotes," +
            "HasFoodToRemove,FoodToRemoveDetails,HasFoodToAdd,FoodToAddDetails,HasFoodToKeepFromPrevious,FoodToKeepFromPreviousDetails," +
            "DesiredMealsCount,DietaryNotes," +
            "WorkoutCommitmentLevel,WorkoutDaysPerWeek,DailySleepHours,DailyWaterIntake,DailyWalkingHours,HasInjuries,InjuriesDetails," +
            "PreferredWorkoutDays,WorkoutGoals,AvailableEquipment,WorkoutLocation,WorkoutNotes"
        )] ClientAssessment clientAssessment)
        {
            // Manually remove validation for navigation properties
            ModelState.Remove("Client");

            // Handle multiple-choice strings (if coming from checkboxes, they might be string arrays)
            // For simplicity, assuming the form directly binds to string, or you'll need custom model binding
            // If using multiple checkboxes for e.g. PreferredWorkoutDays, you'd need to join them here:
            // clientAssessment.PreferredWorkoutDays = string.Join(",", collection.Where(x => x.StartsWith("PreferredWorkoutDays_")).Select(x => x.Replace("PreferredWorkoutDays_", "")));

            if (ModelState.IsValid)
            {
                _context.Add(clientAssessment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Client assessment added successfully.";
                return RedirectToAction("ClientFile", "Clients", new { id = clientAssessment.ClientId });
            }

            ViewBag.Clients = new SelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", clientAssessment.ClientId);
            TempData["ErrorMessage"] = "Please correct the errors in the form.";
            return View(clientAssessment);
        }

        // GET: ClientAssessments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clientAssessment = await _context.ClientAssessments.FindAsync(id);
            if (clientAssessment == null)
            {
                return NotFound();
            }
            ViewBag.Clients = new SelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", clientAssessment.ClientId);
            return View(clientAssessment);
        }

        // POST: ClientAssessments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(
            "Id,ClientId,Timestamp,FormCode,Country,Religion,ServiceGoal,WeightKg,HeightCm,Gender,DateOfBirth,JobProfession," +
            "NeckCircumferenceCm,WaistCircumferenceCm,HipCircumferenceCm,ArmCircumferenceCm,ThighCircumferenceCm," +
            "FrontBodyPhotoPath,SideBodyPhotoPath,BackBodyPhotoPath," +
            "HasHealthProblems,HealthProblemsDetails,HasRecentTests,RecentTestsDetails,IsTakingMedicationsSupplements,MedicationsSupplementsDetails," +
            "HasMedicationAllergies,MedicationAllergiesDetails,HasChronicHereditaryDiseases,ChronicHereditaryDiseasesDetails," +
            "HasPastSurgeries,PastSurgeriesDetails,IsPregnantOrPlanning,IsTakingVitaminsMinerals,VitaminsMineralsDetails,OtherMedicalNotes," +
            "HasFoodToRemove,FoodToRemoveDetails,HasFoodToAdd,FoodToAddDetails,HasFoodToKeepFromPrevious,FoodToKeepFromPreviousDetails," +
            "DesiredMealsCount,DietaryNotes," +
            "WorkoutCommitmentLevel,WorkoutDaysPerWeek,DailySleepHours,DailyWaterIntake,DailyWalkingHours,HasInjuries,InjuriesDetails," +
            "PreferredWorkoutDays,WorkoutGoals,AvailableEquipment,WorkoutLocation,WorkoutNotes"
        )] ClientAssessment clientAssessment)
        {
            if (id != clientAssessment.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Client");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(clientAssessment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Client assessment updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientAssessmentExists(clientAssessment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("ClientFile", "Clients", new { id = clientAssessment.ClientId });
            }
            ViewBag.Clients = new SelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", clientAssessment.ClientId);
            TempData["ErrorMessage"] = "Please correct the errors in the form.";
            return View(clientAssessment);
        }

        // GET: ClientAssessments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clientAssessment = await _context.ClientAssessments
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (clientAssessment == null)
            {
                return NotFound();
            }

            return View(clientAssessment);
        }

        // POST: ClientAssessments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var clientAssessment = await _context.ClientAssessments.FindAsync(id);
            if (clientAssessment != null)
            {
                _context.ClientAssessments.Remove(clientAssessment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Client assessment deleted successfully.";
            }
            return RedirectToAction("ClientFile", "Clients", new { id = clientAssessment?.ClientId });
        }

        private bool ClientAssessmentExists(int id)
        {
            return _context.ClientAssessments.Any(e => e.Id == id);
        }
    }
}