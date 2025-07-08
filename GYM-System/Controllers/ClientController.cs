using GYM_System.Data;
using GYM_System.Models;
using GYM_System.Services;
using GYM_System.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace SuperSheets.Controllers
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

        // GET: Clients/Details/5 - This action is now effectively replaced by ClientFile for a more comprehensive view.
        // Keeping it for potential direct access or if needed elsewhere.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.PackageType) // Eager load PackageType
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.Currency) // Eager load Currency
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.PaymentAccount) // Eager load PaymentAccount
                .Include(c => c.ClientAssessments.OrderByDescending(ca => ca.Timestamp))
                .Include(c => c.ClientUpdates.OrderByDescending(cu => cu.Timestamp))
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/ClientFile/5 - Comprehensive client view
        public async Task<IActionResult> ClientFile(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.PackageType) // Eager load PackageType
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.Currency) // Eager load Currency
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.PaymentAccount) // Eager load PaymentAccount
                .Include(c => c.ClientAssessments.OrderBy(ca => ca.Timestamp))
                .Include(c => c.ClientUpdates.OrderBy(cu => cu.Timestamp))
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
                DietPlans = await _context.DietPlans
                                          .Where(dp => dp.ClientId == id)
                                          .OrderByDescending(dp => dp.CreatedDate)
                                          .ToListAsync(),
                WorkoutPlans = await _context.WorkoutPlans
                                            .Where(wp => wp.ClientId == id)
                                            .OrderByDescending(wp => wp.CreatedDate)
                                            .ToListAsync()
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
            await PopulateSubscriptionDropdowns(); // Populate dropdowns
            return View();
        }

        // POST: Clients/AddSubscription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSubscription([Bind("ClientId,PackageTypeId,StartDate,DurationMonths,Price,CurrencyId,PaymentAccountId,RenewalCount")] Subscription subscription)
        {
            // Remove ModelState for navigation properties and NotMapped properties
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
            // If model state is invalid, re-populate ViewBag for the view
            var client = await _context.Clients.FindAsync(subscription.ClientId);
            if (client != null)
            {
                ViewBag.ClientName = client.Name;
                ViewBag.ClientId = client.Id;
            }
            await PopulateSubscriptionDropdowns(); // Re-populate dropdowns on validation error
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
            await PopulateSubscriptionDropdowns(); // Populate dropdowns
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

            // Remove ModelState for navigation properties and NotMapped properties
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
            // If model state is invalid, re-populate ViewBag for the view
            var client = await _context.Clients.FindAsync(subscription.ClientId);
            if (client != null)
            {
                ViewBag.ClientName = client.Name;
                ViewBag.ClientId = client.Id;
            }
            await PopulateSubscriptionDropdowns(); // Re-populate dropdowns on validation error
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
                .Include(s => s.PackageType) // Eager load for display in delete confirmation
                .Include(s => s.Currency)    // Eager load for display
                .Include(s => s.PaymentAccount) // Eager load for display
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
                                HeightCm = ParseDecimal(row.ElementAtOrDefault(2)),
                                WeightKg = ParseDecimal(row.ElementAtOrDefault(3)),
                                Age = ParseInt(row.ElementAtOrDefault(4)),
                                Gender = row.ElementAtOrDefault(5)?.ToString(),
                                ActivityLevel = row.ElementAtOrDefault(6)?.ToString(),
                                DietaryPreferences = row.ElementAtOrDefault(7)?.ToString(),
                                FitnessGoals = row.ElementAtOrDefault(8)?.ToString(),
                                OtherNotes = row.ElementAtOrDefault(9)?.ToString(),
                                UploadedImageUrl = row.ElementAtOrDefault(10)?.ToString()
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
                                CurrentWeightKg = ParseDecimal(row.ElementAtOrDefault(2)),
                                PhysiqueChanges = row.ElementAtOrDefault(3)?.ToString(),
                                DietAdjustmentsNotes = row.ElementAtOrDefault(4)?.ToString(),
                                ExerciseAdjustmentsNotes = row.ElementAtOrDefault(5)?.ToString(),
                                AdditionalNotes = row.ElementAtOrDefault(6)?.ToString(),
                                UploadedImageUrl = row.ElementAtOrDefault(7)?.ToString()
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
                        }
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Sync complete! Imported {assessmentsImported} new initial assessments and {updatesImported} new updates. Skipped {skippedInvalidFormCode} rows due to invalid Form Code or missing client, and {skippedDuplicates} duplicate entries.";
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
