using GYM_System.Data;
using GYM_System.Models;
using GYM_System.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
                // Check if any subscription is currently active
                if (client.Subscriptions != null && client.Subscriptions.Any(s => s.IsActive))
                {
                    client.Status = SubscriptionStatus.Active;
                }
                else if (client.Subscriptions != null && client.Subscriptions.Any())
                {
                    // If no active but there are subscriptions, they must all be expired
                    client.Status = SubscriptionStatus.Expired;
                }
                else
                {
                    // No subscriptions at all
                    client.Status = SubscriptionStatus.Inactive;
                }
                _context.Update(client);
                await _context.SaveChangesAsync();
            }
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            // Eager load subscriptions for each client
            var clients = await _context.Clients
                                        .Include(c => c.Subscriptions)
                                        .OrderByDescending(c => c.JoinDate) // Order by latest joined clients
                                        .ToListAsync();
            return View(clients);
        }

        // GET: Clients/Details/5 - Show client details and their subscriptions, assessments, and updates
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.Subscriptions)
                .Include(c => c.ClientAssessments.OrderByDescending(ca => ca.Timestamp)) // Include assessments
                .Include(c => c.ClientUpdates.OrderByDescending(cu => cu.Timestamp))     // Include updates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
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
                return RedirectToAction(nameof(Details), new { id = client.Id });
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
                return RedirectToAction(nameof(Details), new { id = client.Id });
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
            return View();
        }

        // POST: Clients/AddSubscription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSubscription([Bind("ClientId,PackageType,StartDate,DurationMonths,Price,Currency,PaymentAccount,RenewalCount")] Subscription subscription)
        {
            ModelState.Remove("Client");
            ModelState.Remove("IsActive");

            if (ModelState.IsValid)
            {
                _context.Add(subscription);
                await _context.SaveChangesAsync();
                await UpdateClientSubscriptionStatus(subscription.ClientId);
                return RedirectToAction(nameof(Details), new { id = subscription.ClientId });
            }
            var client = await _context.Clients.FindAsync(subscription.ClientId);
            if (client != null)
            {
                ViewBag.ClientName = client.Name;
                ViewBag.ClientId = client.Id;
            }
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
            return View(subscription);
        }

        // POST: Clients/EditSubscription/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubscription(int id, [Bind("Id,ClientId,PackageType,StartDate,DurationMonths,Price,Currency,PaymentAccount,RenewalCount")] Subscription subscription)
        {
            if (id != subscription.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Client");
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
                return RedirectToAction(nameof(Details), new { id = subscription.ClientId });
            }
            var client = await _context.Clients.FindAsync(subscription.ClientId);
            if (client != null)
            {
                ViewBag.ClientName = client.Name;
                ViewBag.ClientId = client.Id;
            }
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
            return RedirectToAction(nameof(Details), new { id = clientId });
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
                    // Assuming the first row is headers, skip it for data processing
                    // Expected columns (and their 0-based index) for Initial Assessment:
                    // Timestamp (0), Form Code (1), Height (2), Weight (3), Age (4), Gender (5), Activity Level (6), Dietary Preferences (7), Fitness Goals (8), Other Notes (9), Uploaded Image URL (10)
                    foreach (var row in initialAssessmentData.Skip(1)) // Skip header row
                    {
                        try
                        {
                            // Ensure minimum number of columns are present
                            if (row.Count < 10) continue;

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
                                continue; // Skip if no client with this FormCode exists
                            }

                            // Check for duplicates based on ClientId and Timestamp
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
                                HeightCm = ParseDecimal(row.ElementAtOrDefault(3)),
                                WeightKg = ParseDecimal(row.ElementAtOrDefault(4)),
                                Age = ParseInt(row.ElementAtOrDefault(5)),
                                Gender = row.ElementAtOrDefault(6)?.ToString(),
                                ActivityLevel = row.ElementAtOrDefault(7)?.ToString(),
                                DietaryPreferences = row.ElementAtOrDefault(8)?.ToString(),
                                FitnessGoals = row.ElementAtOrDefault(9)?.ToString(),
                                OtherNotes = row.ElementAtOrDefault(10)?.ToString(),
                                UploadedImageUrl = row.ElementAtOrDefault(11)?.ToString() // Assuming image URL is provided in the sheet
                            };
                            _context.ClientAssessments.Add(assessment);
                            assessmentsImported++;

                            // Update client status if it's "Not Started"
                            if (client.DietStatus == PlanStatus.NotStarted) client.DietStatus = PlanStatus.WaitingForPlan;
                            if (client.WorkoutStatus == PlanStatus.NotStarted) client.WorkoutStatus = PlanStatus.WaitingForPlan;
                            _context.Update(client); // Mark client as modified
                        }
                        catch (Exception ex)
                        {
                            // Log the error but continue processing other rows
                            Console.WriteLine($"Error processing initial assessment row: {ex.Message}");
                            // Consider adding this row to a list of failed imports
                        }
                    }
                }

                // --- Sync Update Forms ---
                var updateFormData = await _googleSheetsService.ReadSheetData(spreadsheetId, updateFormSheetRange);
                if (updateFormData.Any())
                {
                    // Assuming the first row is headers, skip it for data processing
                    // Expected columns (and their 0-based index) for Update Form:
                    // Timestamp (0), Form Code (1), Current Weight (2), Physique Changes (3), Diet Adjustments (4), Exercise Adjustments (5), Additional Notes (6), Uploaded Image URL (7)
                    foreach (var row in updateFormData.Skip(1)) // Skip header row
                    {
                        try
                        {
                            if (row.Count < 7) continue;

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
                                continue; // Skip if no client with this FormCode exists
                            }

                            // Check for duplicates based on ClientId and Timestamp
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
                                CurrentWeightKg = ParseDecimal(row.ElementAtOrDefault(2)),
                                PhysiqueChanges = row.ElementAtOrDefault(3)?.ToString(),
                                DietAdjustmentsNotes = row.ElementAtOrDefault(4)?.ToString(),
                                ExerciseAdjustmentsNotes = row.ElementAtOrDefault(5)?.ToString(),
                                AdditionalNotes = row.ElementAtOrDefault(6)?.ToString(),
                                UploadedImageUrl = row.ElementAtOrDefault(7)?.ToString() // Assuming image URL is provided
                            };
                            _context.ClientUpdates.Add(update);
                            updatesImported++;

                            // Update client status based on updates if they were on plan or waiting
                            if (client.DietStatus == PlanStatus.OnPlan || client.DietStatus == PlanStatus.WaitingForPlan) client.DietStatus = PlanStatus.NeedsUpdateForm;
                            if (client.WorkoutStatus == PlanStatus.OnPlan || client.WorkoutStatus == PlanStatus.WaitingForPlan) client.WorkoutStatus = PlanStatus.NeedsUpdateForm;
                            _context.Update(client); // Mark client as modified
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing update form row: {ex.Message}");
                        }
                    }
                }

                await _context.SaveChangesAsync(); // Save all changes at once

                TempData["SuccessMessage"] = $"Sync complete! Imported {assessmentsImported} new initial assessments and {updatesImported} new updates. Skipped {skippedInvalidFormCode} rows due to invalid Form Code or missing client, and {skippedDuplicates} duplicate entries.";
            }
            catch (Google.Apis.Auth.OAuth2.Responses.TokenResponseException authEx)
            {
                // This typically means the user needs to re-authenticate or permissions are wrong
                TempData["ErrorMessage"] = "Google Sheets Authentication required or failed. Please ensure 'client_secret.json' is correct and try again. You might need to authenticate in your browser.";
                Console.WriteLine($"Auth Error: {authEx.Message}");
            }
            catch (FileNotFoundException)
            {
                TempData["ErrorMessage"] = "client_secret.json not found. Please place it in the GYM_System project root directory.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred during sync: {ex.Message}. Check console for details.";
                Console.WriteLine($"Sync Error: {ex.ToString()}");
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper method to safely parse decimal values
        private decimal? ParseDecimal(object? value)
        {
            if (value != null && decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            return null;
        }

        // Helper method to safely parse int values
        private int? ParseInt(object? value)
        {
            if (value != null && int.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
            {
                return result;
            }
            return null;
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