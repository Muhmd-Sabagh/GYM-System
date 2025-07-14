using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GYM_System.Data;
using GYM_System.Models;
using GYM_System.Services;
using GYM_System.ViewModels;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GYM_System.Controllers
{
    public class ClientsController : Controller
    {
        private readonly GymDbContext _context;
        private readonly GoogleSheetsService _googleSheetsService;

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

            // The Details view now expects the Client model directly, not ClientFileViewModel.
            // So, we pass the client object directly. The view will access its collections.
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
                            // Minimum expected columns for initial assessment. Adjust if your sheet has fewer required fields at the start.
                            // Based on the provided questions, there are 54 fields (0-53).
                            if (row.Count < 54)
                            {
                                skippedInvalidFormCode++; // Or a more specific error for incomplete data
                                continue;
                            }

                            string formCode = row[1]?.ToString()?.Trim() ?? string.Empty;
                            DateTime timestamp = DateTime.TryParseExact(row[0]?.ToString(), "M/d/yyyy H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ts) ? ts : DateTime.MinValue;

                            if (string.IsNullOrEmpty(formCode) || timestamp == DateTime.MinValue)
                            {
                                skippedInvalidFormCode++;
                                continue;
                            }

                            var client = await _context.Clients.FirstOrDefaultAsync(c => c.FormCode == formCode);
                            if (client == null)
                            {
                                skippedInvalidFormCode++;
                                continue;
                            }

                            bool exists = await _context.ClientAssessments
                                .AnyAsync(ca => ca.ClientId == client.Id && ca.Timestamp == timestamp);
                            if (exists)
                            {
                                skippedDuplicates++;
                                continue;
                            }

                            var assessment = new ClientAssessment
                            {
                                ClientId = client.Id,
                                Timestamp = timestamp,
                                FormCode = formCode,

                                // Client Information (Questions 2-9)
                                Country = row.ElementAtOrDefault(2)?.ToString(),
                                Religion = row.ElementAtOrDefault(3)?.ToString(),
                                ServiceGoal = row.ElementAtOrDefault(4)?.ToString(),
                                WeightKg = ParseDecimal(row.ElementAtOrDefault(5)) ?? 0, // Assuming required
                                HeightCm = ParseDecimal(row.ElementAtOrDefault(6)) ?? 0, // Assuming required
                                Gender = row.ElementAtOrDefault(7)?.ToString() ?? string.Empty, // Assuming required
                                DateOfBirth = DateTime.TryParseExact(row.ElementAtOrDefault(8)?.ToString(), "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob) ? dob : DateTime.MinValue, // Assuming required
                                JobProfession = row.ElementAtOrDefault(9)?.ToString(),

                                // Body Assessment (Questions 10-17)
                                NeckCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(10)),
                                WaistCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(11)),
                                HipCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(12)),
                                ArmCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(13)),
                                ThighCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(14)),
                                FrontBodyPhotoPath = row.ElementAtOrDefault(15)?.ToString(),
                                SideBodyPhotoPath = row.ElementAtOrDefault(16)?.ToString(),
                                BackBodyPhotoPath = row.ElementAtOrDefault(17)?.ToString(),

                                // Medical Assessment (Questions 18-26)
                                HasHealthProblems = ParseBool(row.ElementAtOrDefault(18)),
                                HealthProblemsDetails = row.ElementAtOrDefault(19)?.ToString(),
                                HasRecentTests = ParseBool(row.ElementAtOrDefault(20)),
                                RecentTestsDetails = row.ElementAtOrDefault(21)?.ToString(),
                                IsTakingMedicationsSupplements = ParseBool(row.ElementAtOrDefault(22)),
                                MedicationsSupplementsDetails = row.ElementAtOrDefault(23)?.ToString(),
                                HasMedicationAllergies = ParseBool(row.ElementAtOrDefault(24)),
                                MedicationAllergiesDetails = row.ElementAtOrDefault(25)?.ToString(),
                                HasChronicHereditaryDiseases = ParseBool(row.ElementAtOrDefault(26)),
                                ChronicHereditaryDiseasesDetails = row.ElementAtOrDefault(27)?.ToString(),
                                HasPastSurgeries = ParseBool(row.ElementAtOrDefault(28)),
                                PastSurgeriesDetails = row.ElementAtOrDefault(29)?.ToString(),
                                IsPregnantOrPlanning = ParseBoolNullable(row.ElementAtOrDefault(30)), // Nullable bool
                                IsTakingVitaminsMinerals = ParseBool(row.ElementAtOrDefault(31)),
                                VitaminsMineralsDetails = row.ElementAtOrDefault(32)?.ToString(),
                                OtherMedicalNotes = row.ElementAtOrDefault(33)?.ToString(),

                                // Dietary Assessment (Questions 27-31)
                                HasFoodToRemove = ParseBool(row.ElementAtOrDefault(34)),
                                FoodToRemoveDetails = row.ElementAtOrDefault(35)?.ToString(),
                                HasFoodToAdd = ParseBool(row.ElementAtOrDefault(36)),
                                FoodToAddDetails = row.ElementAtOrDefault(37)?.ToString(),
                                HasFoodToKeepFromPrevious = ParseBool(row.ElementAtOrDefault(38)),
                                FoodToKeepFromPreviousDetails = row.ElementAtOrDefault(39)?.ToString(),
                                DesiredMealsCount = ParseInt(row.ElementAtOrDefault(40)) ?? 0, // Assuming required
                                DietaryNotes = row.ElementAtOrDefault(41)?.ToString(),

                                // Workout Assessment (Questions 32-42)
                                WorkoutCommitmentLevel = row.ElementAtOrDefault(42)?.ToString() ?? string.Empty, // Assuming required
                                WorkoutDaysPerWeek = ParseInt(row.ElementAtOrDefault(43)) ?? 0, // Assuming required
                                DailySleepHours = row.ElementAtOrDefault(44)?.ToString() ?? string.Empty, // Assuming required
                                DailyWaterIntake = row.ElementAtOrDefault(45)?.ToString() ?? string.Empty, // Assuming required
                                DailyWalkingHours = row.ElementAtOrDefault(46)?.ToString() ?? string.Empty, // Assuming required
                                HasInjuries = ParseBool(row.ElementAtOrDefault(47)),
                                InjuriesDetails = row.ElementAtOrDefault(48)?.ToString(),
                                PreferredWorkoutDays = row.ElementAtOrDefault(49)?.ToString(), // Comma-separated string
                                WorkoutGoals = row.ElementAtOrDefault(50)?.ToString(), // Comma-separated string
                                AvailableEquipment = row.ElementAtOrDefault(51)?.ToString(), // Comma-separated string
                                WorkoutLocation = row.ElementAtOrDefault(52)?.ToString(), // Comma-separated string
                                WorkoutNotes = row.ElementAtOrDefault(53)?.ToString()
                            };
                            _context.ClientAssessments.Add(assessment);
                            assessmentsImported++;

                            if (client.DietStatus == PlanStatus.NotStarted) client.DietStatus = PlanStatus.WaitingForPlan;
                            if (client.WorkoutStatus == PlanStatus.NotStarted) client.WorkoutStatus = PlanStatus.WaitingForPlan;
                            _context.Update(client);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing initial assessment row: {ex.Message}");
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
                            // Minimum expected columns for client update. Based on your update questions, there are 12 fields (0-11).
                            if (row.Count < 12)
                            {
                                skippedInvalidFormCode++; // Or a more specific error for incomplete data
                                continue;
                            }

                            string formCode = row[1]?.ToString()?.Trim() ?? string.Empty;
                            DateTime timestamp = DateTime.TryParseExact(row[0]?.ToString(), "M/d/yyyy H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ts) ? ts : DateTime.MinValue;

                            if (string.IsNullOrEmpty(formCode) || timestamp == DateTime.MinValue)
                            {
                                skippedInvalidFormCode++;
                                continue;
                            }

                            var client = await _context.Clients.FirstOrDefaultAsync(c => c.FormCode == formCode);
                            if (client == null)
                            {
                                skippedInvalidFormCode++;
                                continue;
                            }

                            bool exists = await _context.ClientUpdates
                                .AnyAsync(cu => cu.ClientId == client.Id && cu.Timestamp == timestamp);
                            if (exists)
                            {
                                skippedDuplicates++;
                                continue;
                            }

                            var update = new ClientUpdate
                            {
                                ClientId = client.Id,
                                Timestamp = timestamp,
                                FormCode = formCode,

                                // Body Assessment Update (from original update section questions 8-15)
                                NeckCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(2)),
                                WaistCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(3)),
                                HipCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(4)),
                                ArmCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(5)),
                                ThighCircumferenceCm = ParseDecimal(row.ElementAtOrDefault(6)),
                                FrontBodyPhotoPath = row.ElementAtOrDefault(7)?.ToString(),
                                SideBodyPhotoPath = row.ElementAtOrDefault(8)?.ToString(),
                                BackBodyPhotoPath = row.ElementAtOrDefault(9)?.ToString(),

                                // Workout Assessment Update (from original update section question 16-17)
                                WorkoutCommitmentLevel = row.ElementAtOrDefault(10)?.ToString(),
                                Notes = row.ElementAtOrDefault(11)?.ToString()
                            };
                            _context.ClientUpdates.Add(update);
                            updatesImported++;

                            if (client.DietStatus == PlanStatus.OnPlan || client.DietStatus == PlanStatus.WaitingForPlan) client.DietStatus = PlanStatus.NeedsUpdateForm;
                            if (client.WorkoutStatus == PlanStatus.OnPlan || client.WorkoutStatus == PlanStatus.WaitingForPlan) client.WorkoutStatus = PlanStatus.NeedsUpdateForm;
                            _context.Update(client);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing update form row: {ex.Message}");
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

        // New helper to parse boolean values from "Yes"/"No" strings
        private bool ParseBool(object? value)
        {
            if (value != null && (value.ToString()?.Equals("نعم", StringComparison.OrdinalIgnoreCase) == true ||
                                  value.ToString()?.Equals("Yes", StringComparison.OrdinalIgnoreCase) == true))
            {
                return true;
            }
            return false;
        }

        // New helper to parse nullable boolean values (for "Pregnant or Planning?")
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