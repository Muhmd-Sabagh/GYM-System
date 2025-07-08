using GYM_System.Data;
using GYM_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GYM_System.Controllers
{
    public class PaymentAccountsController : Controller
    {
        private readonly GymDbContext _context;

        public PaymentAccountsController(GymDbContext context)
        {
            _context = context;
        }

        // GET: PaymentAccounts
        public async Task<IActionResult> Index()
        {
            return View(await _context.PaymentAccounts.OrderBy(pa => pa.Name).ToListAsync());
        }

        // GET: PaymentAccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PaymentAccounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Details,IsActive")] PaymentAccount paymentAccount)
        {
            if (ModelState.IsValid)
            {
                _context.Add(paymentAccount);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Payment Account '{paymentAccount.Name}' added successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(paymentAccount);
        }

        // GET: PaymentAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentAccount = await _context.PaymentAccounts.FindAsync(id);
            if (paymentAccount == null)
            {
                return NotFound();
            }
            return View(paymentAccount);
        }

        // POST: PaymentAccounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Details,IsActive")] PaymentAccount paymentAccount)
        {
            if (id != paymentAccount.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paymentAccount);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Payment Account '{paymentAccount.Name}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentAccountExists(paymentAccount.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(paymentAccount);
        }

        // GET: PaymentAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentAccount = await _context.PaymentAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentAccount == null)
            {
                return NotFound();
            }

            return View(paymentAccount);
        }

        // POST: PaymentAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paymentAccount = await _context.PaymentAccounts.FindAsync(id);
            if (paymentAccount != null)
            {
                try
                {
                    _context.PaymentAccounts.Remove(paymentAccount);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Payment Account '{paymentAccount.Name}' deleted successfully.";
                }
                catch (DbUpdateException ex)
                {
                    TempData["ErrorMessage"] = $"Cannot delete payment account '{paymentAccount.Name}' because it is currently used by one or more subscriptions.";
                    Console.WriteLine($"Error deleting payment account: {ex.Message}");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PaymentAccountExists(int id)
        {
            return _context.PaymentAccounts.Any(e => e.Id == id);
        }
    }
}
