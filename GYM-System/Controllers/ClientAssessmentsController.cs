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
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ClientAssessments.Include(c => c.Client).OrderByDescending(c => c.Timestamp);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ClientAssessments/Details/5
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
        public async Task<IActionResult> Create(int? clientId)
        {
            ViewBag.Clients = new SelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", clientId);

            if (clientId.HasValue)
            {
                var client = await _context.Clients.FindAsync(clientId.Value);
                if (client != null)
                {
                    ViewBag.DefaultFormCode = client.FormCode;
                }
            }

            // Provide sensible default values for required fields to avoid validation errors on initial form load
            var newAssessment = new ClientAssessment
            {
                Timestamp = DateTime.Now,
                DateOfBirth = DateTime.Now.AddYears(-20), // Example default
                WeightKg = 0,
                HeightCm = 0,
                Gender = "ذكر", // Example default
                Country = "Egypt", // Example default
                Religion = "مسلم", // Example default
                ServiceGoal = "حياة صحية", // Example default
                JobProfession = "",
                DailyEffortDescription = "لا يوجد مجهود تقريبا (وظيفة مكتبية مثلا)", // Example default
                DietCommitmentObstacles = "",
                DesiredMainMealsCount = 3, // Example default
                DietFlexibilityPreference = "أريد مرونة في النظام الغذائي", // Example default
                DietBudget = "ميزانية متوسطة", // Example default
                PreferredProteinTypes = "",
                PreferredCarbohydrateTypes = "",
                PreferredHealthyFatTypes = "",
                WorkoutExperience = "",
                ResistanceTrainingDuration = "أول مرة أمارس تمارين المقاومة",
                WorkoutLocation = "جيم",
                AvailableWorkoutDaysCount = 3,
                AvailableWorkoutDays = "",
                PreferredCardioType = "تريدميل (مشاية)",
                DailyStepsCount = "0",
                ReasonForSubscription = ""
            };

            return View(newAssessment);
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
            "HasPastSurgeries,PastSurgeriesDetails,HasInjuries,InjuriesDetails,IsSmoker," +
            "HasPreviousDietCommitment,PreviousDietExperience,DailyEffortDescription,DietCommitmentObstacles,DrinksSpecificBeverages,BeverageConsumptionDetails," +
            "LastDietSystemAvailable,HasFoodAllergies,FoodAllergyDetails,DislikesSpecificFood,DislikedFoodDetails,WantsVitaminsMinerals,AvailableDesiredVitaminsMinerals," +
            "DesiredMainMealsCount,DietFlexibilityPreference,DietBudget," +
            "WorkoutExperience,ResistanceTrainingDuration,PracticesOtherSports,OtherSportsDetails,WorkoutLocation,AvailableHomeEquipment,AvailableWorkoutDaysCount," +
            "HasExerciseDiscomfort,DiscomfortExercisesDetails,PreferredCardioType,DailyStepsCount," +
            "PreviousOnlineTrainingExperience,ReasonForSubscription,OtherNotes"
        )] ClientAssessment clientAssessment)
        {
            ModelState.Remove("Client"); // Remove validation for navigation property

            // --- Manually handle multiple-choice checkbox values from Request.Form ---
            // These fields are not directly bound by [Bind] because they come as multiple values.
            // They need to be joined into a single comma-separated string.
            clientAssessment.PreferredProteinTypes = Request.Form["PreferredProteinTypes"].Any() ? string.Join(",", Request.Form["PreferredProteinTypes"]) : string.Empty;
            clientAssessment.PreferredCarbohydrateTypes = Request.Form["PreferredCarbohydrateTypes"].Any() ? string.Join(",", Request.Form["PreferredCarbohydrateTypes"]) : string.Empty;
            clientAssessment.PreferredHealthyFatTypes = Request.Form["PreferredHealthyFatTypes"].Any() ? string.Join(",", Request.Form["PreferredHealthyFatTypes"]) : string.Empty;
            clientAssessment.AvailableWorkoutDays = Request.Form["AvailableWorkoutDays"].Any() ? string.Join(",", Request.Form["AvailableWorkoutDays"]) : string.Empty;
            // Note: WorkoutGoals and AvailableEquipment are not in your latest ClientAssessment.cs,
            // but were in my previous version. Removing them here to match your provided model.
            // If you intend to add them back, ensure they are in your ClientAssessment.cs first.
            // clientAssessment.WorkoutGoals = Request.Form["WorkoutGoals"].Any() ? string.Join(",", Request.Form["WorkoutGoals"]) : string.Empty;
            // clientAssessment.AvailableEquipment = Request.Form["AvailableEquipment"].Any() ? string.Join(",", Request.Form["AvailableEquipment"]) : string.Empty;


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
            "HasPastSurgeries,PastSurgeriesDetails,HasInjuries,InjuriesDetails,IsSmoker," +
            "HasPreviousDietCommitment,PreviousDietExperience,DailyEffortDescription,DietCommitmentObstacles,DrinksSpecificBeverages,BeverageConsumptionDetails," +
            "LastDietSystemAvailable,HasFoodAllergies,FoodAllergyDetails,DislikesSpecificFood,DislikedFoodDetails,WantsVitaminsMinerals,AvailableDesiredVitaminsMinerals," +
            "DesiredMainMealsCount,DietFlexibilityPreference,DietBudget," +
            "WorkoutExperience,ResistanceTrainingDuration,PracticesOtherSports,OtherSportsDetails,WorkoutLocation,AvailableHomeEquipment,AvailableWorkoutDaysCount," +
            "HasExerciseDiscomfort,DiscomfortExercisesDetails,PreferredCardioType,DailyStepsCount," +
            "PreviousOnlineTrainingExperience,ReasonForSubscription,OtherNotes"
        )] ClientAssessment clientAssessment)
        {
            if (id != clientAssessment.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Client"); // Remove validation for navigation property

            // --- Manually handle multiple-choice checkbox values from Request.Form ---
            clientAssessment.PreferredProteinTypes = Request.Form["PreferredProteinTypes"].Any() ? string.Join(",", Request.Form["PreferredProteinTypes"]) : string.Empty;
            clientAssessment.PreferredCarbohydrateTypes = Request.Form["PreferredCarbohydrateTypes"].Any() ? string.Join(",", Request.Form["PreferredCarbohydrateTypes"]) : string.Empty;
            clientAssessment.PreferredHealthyFatTypes = Request.Form["PreferredHealthyFatTypes"].Any() ? string.Join(",", Request.Form["PreferredHealthyFatTypes"]) : string.Empty;
            clientAssessment.AvailableWorkoutDays = Request.Form["AvailableWorkoutDays"].Any() ? string.Join(",", Request.Form["AvailableWorkoutDays"]) : string.Empty;
            // Note: WorkoutGoals and AvailableEquipment were removed to match your latest ClientAssessment.cs
            // clientAssessment.WorkoutGoals = Request.Form["WorkoutGoals"].Any() ? string.Join(",", Request.Form["WorkoutGoals"]) : string.Empty;
            // clientAssessment.AvailableEquipment = Request.Form["AvailableEquipment"].Any() ? string.Join(",", Request.Form["AvailableEquipment"]) : string.Empty;


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