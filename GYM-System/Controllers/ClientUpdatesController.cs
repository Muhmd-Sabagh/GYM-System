using GYM_System.Data;
using GYM_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GYM_System.Controllers
{
    public class ClientUpdatesController : Controller
    {
        private readonly GymDbContext _context;

        public ClientUpdatesController(GymDbContext context)
        {
            _context = context;
        }

        // GET: ClientUpdates
        // This Index will show all updates, but typically you'd navigate from ClientFile
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ClientUpdates.Include(c => c.Client).OrderByDescending(c => c.Timestamp);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ClientUpdates/Details/5 (Optional, if you want a dedicated details page)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clientUpdate = await _context.ClientUpdates
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (clientUpdate == null)
            {
                return NotFound();
            }

            return View(clientUpdate);
        }

        // GET: ClientUpdates/Create
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

        // POST: ClientUpdates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(
            "ClientId,Timestamp,FormCode," +
            "NeckCircumferenceCm,WaistCircumferenceCm,HipCircumferenceCm,ArmCircumferenceCm,ThighCircumferenceCm," +
            "FrontBodyPhotoPath,SideBodyPhotoPath,BackBodyPhotoPath," +
            "WorkoutCommitmentLevel,Notes"
        )] ClientUpdate clientUpdate)
        {
            // Manually remove validation for navigation properties
            ModelState.Remove("Client");

            if (ModelState.IsValid)
            {
                _context.Add(clientUpdate);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Client update added successfully.";
                return RedirectToAction("ClientFile", "Clients", new { id = clientUpdate.ClientId });
            }

            ViewBag.Clients = new SelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", clientUpdate.ClientId);
            TempData["ErrorMessage"] = "Please correct the errors in the form.";
            return View(clientUpdate);
        }

        // GET: ClientUpdates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clientUpdate = await _context.ClientUpdates.FindAsync(id);
            if (clientUpdate == null)
            {
                return NotFound();
            }
            ViewBag.Clients = new SelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", clientUpdate.ClientId);
            return View(clientUpdate);
        }

        // POST: ClientUpdates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(
            "Id,ClientId,Timestamp,FormCode," +
            "NeckCircumferenceCm,WaistCircumferenceCm,HipCircumferenceCm,ArmCircumferenceCm,ThighCircumferenceCm," +
            "FrontBodyPhotoPath,SideBodyPhotoPath,BackBodyPhotoPath," +
            "WorkoutCommitmentLevel,Notes"
        )] ClientUpdate clientUpdate)
        {
            if (id != clientUpdate.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Client");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(clientUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Client update updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientUpdateExists(clientUpdate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("ClientFile", "Clients", new { id = clientUpdate.ClientId });
            }
            ViewBag.Clients = new SelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", clientUpdate.ClientId);
            TempData["ErrorMessage"] = "Please correct the errors in the form.";
            return View(clientUpdate);
        }

        // GET: ClientUpdates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clientUpdate = await _context.ClientUpdates
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (clientUpdate == null)
            {
                return NotFound();
            }

            return View(clientUpdate);
        }

        // POST: ClientUpdates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var clientUpdate = await _context.ClientUpdates.FindAsync(id);
            if (clientUpdate != null)
            {
                _context.ClientUpdates.Remove(clientUpdate);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Client update deleted successfully.";
            }
            return RedirectToAction("ClientFile", "Clients", new { id = clientUpdate?.ClientId });
        }

        private bool ClientUpdateExists(int id)
        {
            return _context.ClientUpdates.Any(e => e.Id == id);
        }
    }
}
