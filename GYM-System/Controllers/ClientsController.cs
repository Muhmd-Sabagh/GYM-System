using GYM_System.Data;
using GYM_System.Models;
using GYM_System.Services;
using GYM_System.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GYM_System.Controllers
{
    public class ClientsController : Controller
    {
        readonly GymDbContext _context;
        readonly GoogleSheetsService _googleSheetsService;

        public ClientsController(GymDbContext context, GoogleSheetsService googleSheetsService)
        {
            _context = context;
            _googleSheetsService = googleSheetsService;
        }

        // Helper to update client status based on subscriptions
        private async Task UpdateClientSubscriptionStatus(int clientId)
        {
            var client = await _context.Clients
                                       .Include(c => c.Subscriptions)
                                       .FirstOrDefaultAsync(c => c.Id == clientId);

            if (client != null)
            {
                if (client.Subscriptions != null && client.Subscriptions.Any(s => s.IsActive))
                {
                    client.Status = SubscriptionStatus.Active;
                }
                else if (client.Subscriptions != null && client.Subscriptions.Any())
                {
                    client.Status = SubscriptionStatus.Expired;
                }
                else
                {
                    client.Status = SubscriptionStatus.Inactive;
                }
                _context.Update(client);
                await _context.SaveChangesAsync();
            }
        }

        // Helper to populate dropdowns
        private async Task PopulateSubscriptionDropdowns()
        {
            ViewBag.Packages = new SelectList(await _context.Packages.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            ViewBag.Currencies = new SelectList(await _context.Currencies.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(), "Id", "Code");
            ViewBag.PaymentAccounts = new SelectList(await _context.PaymentAccounts.Where(pa => pa.IsActive).OrderBy(pa => pa.Name).ToListAsync(), "Id", "Name");
        }


        // GET: Clients
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients
                                        .Include(c => c.Subscriptions)
                                        .OrderByDescending(c => c.JoinDate)
                                        .ToListAsync();
            return View(clients);
        }

        // GET: Clients/Details/5 - Updated to include all related data
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.PackageType)
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.Currency)
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.PaymentAccount)
                .Include(c => c.ClientAssessments.OrderByDescending(ca => ca.Timestamp)) // Include assessments
                .Include(c => c.ClientUpdates.OrderByDescending(cu => cu.Timestamp))     // Include updates
                .Include(c => c.DietPlans.OrderByDescending(dp => dp.CreatedDate))       // Include diet plans
                .Include(c => c.WorkoutPlans.OrderByDescending(wp => wp.CreatedDate))     // Include workout plans
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/ClientFile/5 - Comprehensive client view (already updated)
        public async Task<IActionResult> ClientFile(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.PackageType)
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.Currency)
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.PaymentAccount)
                .Include(c => c.ClientAssessments.OrderBy(ca => ca.Timestamp))
                .Include(c => c.ClientUpdates.OrderBy(cu => cu.Timestamp))
                .Include(c => c.DietPlans) // Ensure DietPlans are included for ClientFileViewModel
                .Include(c => c.WorkoutPlans) // Ensure WorkoutPlans are included for ClientFileViewModel
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            var viewModel = new ClientFileViewModel(client)
            {
                Subscriptions = client.Subscriptions?.OrderByDescending(s => s.StartDate).ToList() ?? new List<Subscription>(),
                ClientAssessments = client.ClientAssessments?.ToList() ?? new List<ClientAssessment>(),
                ClientUpdates = client.ClientUpdates?.ToList() ?? new List<ClientUpdate>(),
                DietPlans = client.DietPlans?.ToList() ?? new List<DietPlan>(), // Use already included data
                WorkoutPlans = client.WorkoutPlans?.ToList() ?? new List<WorkoutPlan>() // Use already included data
            };

            return View(viewModel);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Phone,Email")] Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ClientFile), new { id = client.Id });
            }
            return View(client);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientCode,Name,Phone,Email,JoinDate,FormCode,Status,DietStatus,WorkoutStatus")] Client client)
        {
            if (id != client.Id)
            {
                return NotFound();
            }

            ModelState.Remove("ClientCode");
            ModelState.Remove("JoinDate");
            ModelState.Remove("FormCode");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingClient = await _context.Clients.FindAsync(id);
                    if (existingClient == null)
                    {
                        return NotFound();
                    }

                    existingClient.Name = client.Name;
                    existingClient.Phone = client.Phone;
                    existingClient.Email = client.Email;
                    existingClient.Status = client.Status;
                    existingClient.DietStatus = client.DietStatus;
                    existingClient.WorkoutStatus = client.WorkoutStatus;

                    _context.Update(existingClient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ClientFile), new { id = client.Id });
            }
            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/AddSubscription/5
        public async Task<IActionResult> AddSubscription(int? clientId)
        {
            if (clientId == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(clientId);
            if (client == null)
            {
                return NotFound();
            }

            ViewBag.ClientName = client.Name;
            ViewBag.ClientId = client.Id;
            await PopulateSubscriptionDropdowns();
            return View();
        }

        // POST: Clients/AddSubscription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSubscription([Bind("ClientId,PackageTypeId,StartDate,DurationMonths,Price,CurrencyId,PaymentAccountId,RenewalCount")] Subscription subscription)
        {
            ModelState.Remove("Client");
            ModelState.Remove("PackageType");
            ModelState.Remove("Currency");
            ModelState.Remove("PaymentAccount");
            ModelState.Remove("IsActive");

            if (ModelState.IsValid)
            {
                _context.Add(subscription);
                await _context.SaveChangesAsync();
                await UpdateClientSubscriptionStatus(subscription.ClientId);
                return RedirectToAction(nameof(ClientFile), new { id = subscription.ClientId });
            }
            var client = await _context.Clients.FindAsync(subscription.ClientId);
            if (client != null)
            {
                ViewBag.ClientName = client.Name;
                ViewBag.ClientId = client.Id;
            }
            await PopulateSubscriptionDropdowns();
            return View(subscription);
        }

        // GET: Clients/EditSubscription/5
        public async Task<IActionResult> EditSubscription(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subscription = await _context.Subscriptions
                                            .Include(s => s.Client)
                                            .FirstOrDefaultAsync(s => s.Id == id);
            if (subscription == null)
            {
                return NotFound();
            }

            ViewBag.ClientName = subscription.Client?.Name;
            ViewBag.ClientId = subscription.ClientId;
            await PopulateSubscriptionDropdowns();
            return View(subscription);
        }

        // POST: Clients/EditSubscription/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubscription(int id, [Bind("Id,ClientId,PackageTypeId,StartDate,DurationMonths,Price,CurrencyId,PaymentAccountId,RenewalCount")] Subscription subscription)
        {
            if (id != subscription.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Client");
            ModelState.Remove("PackageType");
            ModelState.Remove("Currency");
            ModelState.Remove("PaymentAccount");
            ModelState.Remove("IsActive");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subscription);
                    await _context.SaveChangesAsync();
                    await UpdateClientSubscriptionStatus(subscription.ClientId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubscriptionExists(subscription.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ClientFile), new { id = subscription.ClientId });
            }
            var client = await _context.Clients.FindAsync(subscription.ClientId);
            if (client != null)
            {
                ViewBag.ClientName = client.Name;
                ViewBag.ClientId = client.Id;
            }
            await PopulateSubscriptionDropdowns();
            return View(subscription);
        }

        // GET: Clients/DeleteSubscription/5
        public async Task<IActionResult> DeleteSubscription(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subscription = await _context.Subscriptions
                .Include(s => s.Client)
                .Include(s => s.PackageType)
                .Include(s => s.Currency)
                .Include(s => s.PaymentAccount)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subscription == null)
            {
                return NotFound();
            }

            return View(subscription);
        }

        // POST: Clients/DeleteSubscription/5
        [HttpPost, ActionName("DeleteSubscription")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubscriptionConfirmed(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }

            var clientId = subscription.ClientId;
            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
            await UpdateClientSubscriptionStatus(clientId);
            return RedirectToAction(nameof(ClientFile), new { id = clientId });
        }


        // POST: Clients/SyncGoogleForms
        [HttpPost]
        public async Task<IActionResult> SyncGoogleForms(string spreadsheetId, string initialAssessmentSheetRange, string updateFormSheetRange)
        {
            int assessmentsImported = 0;
            int updatesImported = 0;
            int skippedInvalidFormCode = 0;
            int skippedDuplicates = 0;

            try
            {
                // --- Sync Initial Assessment Forms ---
                var initialAssessmentData = await _googleSheetsService.ReadSheetData(spreadsheetId, initialAssessmentSheetRange);
                if (initialAssessmentData.Any())
                {
                    foreach (var row in initialAssessmentData.Skip(1)) // Skip header row
                    {
                        try
                        {
                            // Expected columns for initial assessment: 67 (0 to 66)  (41 not nullable)
                            if (row.Count < 41) // Adjusted count
                            {
                                skippedInvalidFormCode++; // Or a more specific error for incomplete data
                                Console.WriteLine($"Skipped initial assessment row due to insufficient columns: Expected 41 at least, got {row.Count}. Row: {string.Join(",", row)}");
                                continue;
                            }

                            string formCode = row.ElementAtOrDefault(1)?.ToString()?.Trim() ?? string.Empty;
                            DateTime timestamp = DateTime.TryParseExact(row.ElementAtOrDefault(0)?.ToString(), "M/d/yyyy H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ts) ? ts : DateTime.MinValue;

                            if (string.IsNullOrEmpty(formCode) || timestamp == DateTime.MinValue)
                            {
                                skippedInvalidFormCode++;
                                Console.WriteLine($"Skipped initial assessment row due to invalid Form Code or Timestamp. FormCode: '{formCode}', Timestamp: '{row.ElementAtOrDefault(0)}'. Row: {string.Join(",", row)}");
                                continue;
                            }

                            var client = await _context.Clients.FirstOrDefaultAsync(c => c.FormCode == formCode);
                            if (client == null)
                            {
                                skippedInvalidFormCode++;
                                Console.WriteLine($"Skipped initial assessment row: Client with FormCode '{formCode}' not found. Row: {string.Join(",", row)}");
                                continue;
                            }

                            bool exists = await _context.ClientAssessments
                                .AnyAsync(ca => ca.ClientId == client.Id && ca.Timestamp == timestamp);
                            if (exists)
                            {
                                skippedDuplicates++;
                                Console.WriteLine($"Skipped initial assessment row: Duplicate entry for ClientId {client.Id} at Timestamp {timestamp}. Row: {string.Join(",", row)}");
                                continue;
                            }

                            var assessment = new ClientAssessment
                            {
                                ClientId = client.Id,
                                Timestamp = timestamp,
                                FormCode = formCode,

                                // Client Information (Questions 2-9)
                                Country = row.ElementAtOrDefault(2)?.ToString() ?? string.Empty,
                                Religion = row.ElementAtOrDefault(3)?.ToString() ?? string.Empty,
                                ServiceGoal = row.ElementAtOrDefault(4)?.ToString() ?? string.Empty,
                                WeightKg = ParseDecimal(row.ElementAtOrDefault(5)) ?? 0,
                                HeightCm = ParseDecimal(row.ElementAtOrDefault(6)) ?? 0,
                                Gender = row.ElementAtOrDefault(7)?.ToString() ?? string.Empty,
                                DateOfBirth = DateTime.TryParseExact(row.ElementAtOrDefault(8)?.ToString(), "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob) ? dob : DateTime.MinValue,
                                JobProfession = row.ElementAtOrDefault(9)?.ToString() ?? string.Empty,

                                // Body Assessment (Questions 10-17)
                                NeckCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(10)),
                                WaistCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(11)),
                                HipCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(12)),
                                ArmCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(13)),
                                ThighCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(14)),
                                FrontBodyPhotoPath = row.ElementAtOrDefault(15)?.ToString(),
                                SideBodyPhotoPath = row.ElementAtOrDefault(16)?.ToString(),
                                BackBodyPhotoPath = row.ElementAtOrDefault(17)?.ToString(),

                                // Medical Assessment (Questions 18-32)
                                HasHealthProblems = ParseBool(row.ElementAtOrDefault(18)),
                                HealthProblemsDetails = row.ElementAtOrDefault(19)?.ToString(),
                                HasRecentTests = ParseBool(row.ElementAtOrDefault(20)),
                                RecentTestsDetails = row.ElementAtOrDefault(21)?.ToString(),
                                IsTakingMedicationsSupplements = ParseBool(row.ElementAtOrDefault(22)),
                                MedicationsSupplementsDetails = row.ElementAtOrDefault(23)?.ToString(),
                                HasMedicationAllergies = ParseBool(row.ElementAtOrDefault(24)),
                                MedicationAllergiesDetails = row.ElementAtOrDefault(25)?.ToString(),
                                HasChronicHereditaryDiseases = ParseBool(row.ElementAtOrDefault(26)), // Assuming this maps to Q26 in markdown (duplicate was Q24)
                                ChronicHereditaryDiseasesDetails = row.ElementAtOrDefault(27)?.ToString(),
                                HasPastSurgeries = ParseBool(row.ElementAtOrDefault(28)),
                                PastSurgeriesDetails = row.ElementAtOrDefault(29)?.ToString(),
                                HasInjuries = ParseBool(row.ElementAtOrDefault(30)),
                                InjuriesDetails = row.ElementAtOrDefault(31)?.ToString(),
                                IsSmoker = ParseBool(row.ElementAtOrDefault(32)),

                                // Dietary Assessment (Questions 33-51)
                                HasPreviousDietCommitment = ParseBool(row.ElementAtOrDefault(33)),
                                PreviousDietExperience = row.ElementAtOrDefault(34)?.ToString(),
                                DailyEffortDescription = row.ElementAtOrDefault(35)?.ToString() ?? string.Empty,
                                DietCommitmentObstacles = row.ElementAtOrDefault(36)?.ToString() ?? string.Empty,
                                DrinksSpecificBeverages = ParseBool(row.ElementAtOrDefault(37)),
                                BeverageConsumptionDetails = row.ElementAtOrDefault(38)?.ToString(),
                                LastDietSystemAvailable = row.ElementAtOrDefault(39)?.ToString(),
                                HasFoodAllergies = ParseBool(row.ElementAtOrDefault(40)),
                                FoodAllergyDetails = row.ElementAtOrDefault(41)?.ToString(),
                                DislikesSpecificFood = ParseBool(row.ElementAtOrDefault(42)),
                                DislikedFoodDetails = row.ElementAtOrDefault(43)?.ToString(),
                                WantsVitaminsMinerals = ParseBool(row.ElementAtOrDefault(44)),
                                AvailableDesiredVitaminsMinerals = row.ElementAtOrDefault(45)?.ToString(),
                                DesiredMainMealsCount = ParseInt(row.ElementAtOrDefault(46)) ?? 0,
                                DietFlexibilityPreference = row.ElementAtOrDefault(47)?.ToString() ?? string.Empty,
                                DietBudget = row.ElementAtOrDefault(48)?.ToString() ?? string.Empty,
                                PreferredProteinTypes = row.ElementAtOrDefault(49)?.ToString() ?? string.Empty,
                                PreferredCarbohydrateTypes = row.ElementAtOrDefault(50)?.ToString() ?? string.Empty,
                                PreferredHealthyFatTypes = row.ElementAtOrDefault(51)?.ToString() ?? string.Empty,

                                // Workout Assessment (Questions 52-63)
                                WorkoutExperience = row.ElementAtOrDefault(52)?.ToString() ?? string.Empty,
                                ResistanceTrainingDuration = row.ElementAtOrDefault(53)?.ToString() ?? string.Empty,
                                PracticesOtherSports = ParseBool(row.ElementAtOrDefault(54)),
                                OtherSportsDetails = row.ElementAtOrDefault(55)?.ToString(),
                                WorkoutLocation = row.ElementAtOrDefault(56)?.ToString() ?? string.Empty,
                                AvailableHomeEquipment = row.ElementAtOrDefault(57)?.ToString(),
                                AvailableWorkoutDaysCount = ParseInt(row.ElementAtOrDefault(58)) ?? 0,
                                AvailableWorkoutDays = row.ElementAtOrDefault(59)?.ToString() ?? string.Empty, // multiple choices
                                HasExerciseDiscomfort = ParseBool(row.ElementAtOrDefault(60)),
                                DiscomfortExercisesDetails = row.ElementAtOrDefault(61)?.ToString(),
                                PreferredCardioType = row.ElementAtOrDefault(62)?.ToString() ?? string.Empty,
                                DailyStepsCount = row.ElementAtOrDefault(63)?.ToString() ?? string.Empty,

                                // General Assessment (Questions 64-66)
                                PreviousOnlineTrainingExperience = row.ElementAtOrDefault(64)?.ToString(),
                                ReasonForSubscription = row.ElementAtOrDefault(65)?.ToString() ?? string.Empty,
                                OtherNotes = row.ElementAtOrDefault(66)?.ToString()
                            };
                            _context.ClientAssessments.Add(assessment);
                            assessmentsImported++;

                            if (client.DietStatus == PlanStatus.NotStarted) client.DietStatus = PlanStatus.WaitingForPlan;
                            if (client.WorkoutStatus == PlanStatus.NotStarted) client.WorkoutStatus = PlanStatus.WaitingForPlan;
                            _context.Update(client);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing initial assessment row: {ex.Message}. Row: {string.Join(",", row)}");
                            // Consider logging more details like the row content for debugging
                        }
                    }
                }

                // --- Sync Update Forms ---
                var updateFormData = await _googleSheetsService.ReadSheetData(spreadsheetId, updateFormSheetRange);
                if (updateFormData.Any())
                {
                    foreach (var row in updateFormData.Skip(1)) // Skip header row
                    {
                        try
                        {
                            // Expected columns for client update: 33 (0 to 32) (15 not nullable)
                            if (row.Count < 15) // Adjusted count
                            {
                                skippedInvalidFormCode++; // Or a more specific error for incomplete data
                                Console.WriteLine($"Skipped client update row due to insufficient columns: Expected 15 at least, got {row.Count}. Row: {string.Join(",", row)}");
                                continue;
                            }

                            string formCode = row.ElementAtOrDefault(1)?.ToString()?.Trim() ?? string.Empty;
                            DateTime timestamp = DateTime.TryParseExact(row.ElementAtOrDefault(0)?.ToString(), "M/d/yyyy H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ts) ? ts : DateTime.MinValue;

                            if (string.IsNullOrEmpty(formCode) || timestamp == DateTime.MinValue)
                            {
                                skippedInvalidFormCode++;
                                Console.WriteLine($"Skipped client update row due to invalid Form Code or Timestamp. FormCode: '{formCode}', Timestamp: '{row.ElementAtOrDefault(0)}'. Row: {string.Join(",", row)}");
                                continue;
                            }

                            var client = await _context.Clients.FirstOrDefaultAsync(c => c.FormCode == formCode);
                            if (client == null)
                            {
                                skippedInvalidFormCode++;
                                Console.WriteLine($"Skipped client update row: Client with FormCode '{formCode}' not found. Row: {string.Join(",", row)}");
                                continue;
                            }

                            bool exists = await _context.ClientUpdates
                                .AnyAsync(cu => cu.ClientId == client.Id && cu.Timestamp == timestamp);
                            if (exists)
                            {
                                skippedDuplicates++;
                                Console.WriteLine($"Skipped client update row: Duplicate entry for ClientId {client.Id} at Timestamp {timestamp}. Row: {string.Join(",", row)}");
                                continue;
                            }

                            var update = new ClientUpdate
                            {
                                ClientId = client.Id,
                                Timestamp = timestamp,
                                FormCode = formCode,

                                CurrentWeightKg = ParseDecimal(row.ElementAtOrDefault(2)) ?? 0,

                                // Dietary Assessment Update (Questions 3-11)
                                DietCommitmentLevel = row.ElementAtOrDefault(11)?.ToString() ?? string.Empty,
                                HasFoodToRemove = ParseBool(row.ElementAtOrDefault(12)),
                                FoodToRemoveDetails = row.ElementAtOrDefault(13)?.ToString(),
                                HasFoodToAdd = ParseBool(row.ElementAtOrDefault(14)),
                                FoodToAddDetails = row.ElementAtOrDefault(15)?.ToString(),
                                HasFoodToKeepFromPrevious = ParseBool(row.ElementAtOrDefault(16)),
                                FoodToKeepFromPreviousDetails = row.ElementAtOrDefault(17)?.ToString(),
                                DesiredMealsCount = ParseInt(row.ElementAtOrDefault(18)) ?? 0,
                                DietaryNotes = row.ElementAtOrDefault(19)?.ToString(),

                                // Body Assessment Update (Questions 12-19)
                                NeckCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(3)),
                                WaistCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(4)),
                                HipCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(5)),
                                ArmCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(6)),
                                ThighCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(7)),
                                FrontBodyPhotoPath = row.ElementAtOrDefault(8)?.ToString(),
                                SideBodyPhotoPath = row.ElementAtOrDefault(9)?.ToString(),
                                BackBodyPhotoPath = row.ElementAtOrDefault(10)?.ToString(),

                                // Workout Assessment Update (Questions 20-31)
                                WorkoutCommitmentLevel = row.ElementAtOrDefault(20)?.ToString() ?? string.Empty,
                                PreviousWorkoutSystemExperience = row.ElementAtOrDefault(21)?.ToString() ?? string.Empty,
                                HasWeightRepsDevelopment = ParseBool(row.ElementAtOrDefault(22)),
                                IsTrainingVolumeSuitable = ParseBool(row.ElementAtOrDefault(23)),
                                DesiredTrainingVolumeAdjustment = row.ElementAtOrDefault(24)?.ToString(),
                                IsTrainingIntensitySuitable = ParseBool(row.ElementAtOrDefault(25)),
                                DesiredTrainingIntensityAdjustment = row.ElementAtOrDefault(26)?.ToString(),
                                HasExerciseDiscomfort = ParseBool(row.ElementAtOrDefault(27)),
                                DiscomfortExerciseName = row.ElementAtOrDefault(28)?.ToString(),
                                AvailableWorkoutDaysCount = ParseInt(row.ElementAtOrDefault(29)) ?? 0,
                                WorkoutLocation = row.ElementAtOrDefault(30)?.ToString() ?? string.Empty,
                                AvailableHomeEquipment = row.ElementAtOrDefault(31)?.ToString(),
                                Notes = row.ElementAtOrDefault(32)?.ToString()
                            };
                            _context.ClientUpdates.Add(update);
                            updatesImported++;

                            // Update client status based on latest update
                            if (client.DietStatus == PlanStatus.OnPlan || client.DietStatus == PlanStatus.WaitingForPlan) client.DietStatus = PlanStatus.NeedsUpdateForm;
                            if (client.WorkoutStatus == PlanStatus.OnPlan || client.WorkoutStatus == PlanStatus.WaitingForPlan) client.WorkoutStatus = PlanStatus.NeedsUpdateForm;
                            _context.Update(client);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing update form row: {ex.Message}. Row: {string.Join(",", row)}");
                            // Consider logging more details like the row content for debugging
                        }
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Sync complete! Imported {assessmentsImported} new initial assessments and {updatesImported} new updates. Skipped {skippedInvalidFormCode} rows due to invalid Form Code/missing client/incomplete data, and {skippedDuplicates} duplicate entries.";
            }
            catch (Google.Apis.Auth.OAuth2.Responses.TokenResponseException authEx)
            {
                TempData["ErrorMessage"] = "Google Sheets Authentication required or failed. Please ensure 'client_secret.json' is correct and try again. You might need to authenticate in your browser.";
                Console.WriteLine($"Auth Error: {authEx.Message}");
            }
            catch (FileNotFoundException)
            {
                TempData["ErrorMessage"] = "client_secret.json not found. Please place it in the SuperSheets project root directory.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred during sync: {ex.Message}. Check console for details.";
                Console.WriteLine($"Sync Error: {ex.ToString()}");
            }
            return RedirectToAction(nameof(Index));
        }

        private decimal? ParseDecimal(object? value)
        {
            if (value != null && decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            return null;
        }

        private int? ParseInt(object? value)
        {
            if (value != null && int.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
            {
                return result;
            }
            return null;
        }

        // Helper to parse boolean values from "نعم"/"Yes" strings (required booleans)
        private bool ParseBool(object? value)
        {
            if (value != null && (value.ToString()?.Equals("نعم", StringComparison.OrdinalIgnoreCase) == true ||
                                  value.ToString()?.Equals("Yes", StringComparison.OrdinalIgnoreCase) == true))
            {
                return true;
            }
            return false;
        }

        // Helper to parse nullable boolean values (if any optional bools were added, like IsPregnantOrPlanning)
        private bool? ParseBoolNullable(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return null;
            }
            if (value.ToString()?.Equals("نعم", StringComparison.OrdinalIgnoreCase) == true ||
                value.ToString()?.Equals("Yes", StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }
            if (value.ToString()?.Equals("لا", StringComparison.OrdinalIgnoreCase) == true ||
                value.ToString()?.Equals("No", StringComparison.OrdinalIgnoreCase) == true)
            {
                return false;
            }
            return null; // For any other unexpected value
        }


        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }

        private bool SubscriptionExists(int id)
        {
            return _context.Subscriptions.Any(e => e.Id == id);
        }
    }
}